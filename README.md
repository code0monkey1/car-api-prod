# Car API — .NET 10 REST API with Google Sheets & Drive Integration

A REST API built with **.NET 10 ASP.NET Core** that stores car records in **Google Sheets** and supports optional image links via **Google Drive**. Includes LINQ-based filtering, Swagger UI, and Docker deployment.

---

## Table of Contents

1. [What This Project Does](#what-this-project-does)
2. [Why Google Sheets as a Database?](#why-google-sheets-as-a-database)
3. [Architecture & Request Flow](#architecture--request-flow)
4. [Tech Stack & Why Each Was Chosen](#tech-stack--why-each-was-chosen)
5. [Project Structure](#project-structure)
6. [Data Model](#data-model)
7. [API Endpoints](#api-endpoints)
8. [How LINQ Filtering Works](#how-linq-filtering-works)
9. [How Google Authentication Works](#how-google-authentication-works)
10. [Design Decisions & Trade-offs](#design-decisions--trade-offs)
11. [Local Development](#local-development)
12. [Configuration](#configuration)
13. [Google Cloud Setup](#google-cloud-setup)
14. [Deployment to Render.com](#deployment-to-rendercom)
15. [Testing](#testing)
16. [Known Limitations](#known-limitations)

---

## What This Project Does

This API lets you:

1. **Add a car** (`POST /api/cars`) — sends car details to the API, which appends a new row to a Google Sheet
2. **Query cars** (`GET /api/cars`) — fetches all rows from the sheet and applies LINQ filters in memory before returning results
3. **Clear all cars** (`DELETE /api/cars`) — clears all data rows from the sheet while preserving the header row

All data lives in a real Google Sheet — no local database. Any change made via the API is immediately visible in the spreadsheet, and vice versa.

---

## Why Google Sheets as a Database?

This is a deliberate architectural choice, not a workaround. Google Sheets was chosen because:

- **Visibility** — non-technical stakeholders can view, edit, and share data without any tooling
- **No infrastructure** — no database server to provision, maintain, or pay for
- **Live sync** — changes are visible instantly at `sheets.google.com`
- **Structured storage** — rows map cleanly to objects; columns map to fields
- **API v4** — Google provides a well-documented REST API with official .NET client libraries

The trade-off is that Sheets is not designed for high-volume writes or complex joins. For this use case (a demo API with dozens to thousands of records), it is entirely appropriate.

---

## Architecture & Request Flow

```
Client (Swagger UI / curl / Postman)
           │
           ▼
  ASP.NET Core Web API
  CarsController.cs
           │
     ┌─────┴──────┐
     │             │
     ▼             ▼
GoogleSheets    GoogleDrive
Service.cs      Service.cs
     │             │
     ▼             ▼
 Sheets API    Drive API
   (v4)          (v3)
     │
     ▼
 Google Sheet
 "Cars" tab
 Rows 2–N
```

### What happens on POST /api/cars (step by step)

1. **Request arrives** at `CarsController.CreateCar()` as `multipart/form-data`
2. **Input validation** runs automatically via `DataAnnotations` on `CarCreateRequest` (e.g. year must be 1900–2030, price must be 0–1,000,000). If invalid, ASP.NET returns `400 Bad Request` before your code even runs.
3. **A `Car` domain object is created** with a new `Guid` as `Id` and `DateTime.UtcNow` as `CreatedAt`
4. **Image handling (optional):**
   - If `imageUrl` text is provided → stored directly on the car object
   - If a file is uploaded → uploaded to Google Drive via `GoogleDriveService.UploadFileAsync()`, which returns a `webViewLink` URL stored in `car.ImageUrl`
5. **`GoogleSheetsService.AppendCarAsync()`** is called. It builds a list of 8 values (`[Id, Make, Model, Year, Price, Color, ImageUrl, CreatedAt]`) and calls the Sheets API `Append` method, which adds a new row to the `Cars` tab
6. **`201 Created`** is returned with the full car object as JSON

### What happens on GET /api/cars (step by step)

1. **Request arrives** at `CarsController.GetCars()` with optional query parameters
2. **`GoogleSheetsService.GetAllCarsAsync()`** is called. It reads range `Cars!A:H` from the spreadsheet — this returns ALL rows including headers as a 2D list
3. **Header row is skipped** (`.Skip(1)`)
4. **Each row is mapped** to a `Car` object. Rows with an empty `Id` field (blank rows) are skipped
5. **LINQ filtering** is applied in memory using `AsQueryable()` — one `.Where()` clause per active filter
6. **`.Take(limit)`** caps the result set (default 100)
7. **`200 OK`** is returned with the filtered list as JSON

---

## Tech Stack & Why Each Was Chosen

| Technology | Why |
|---|---|
| **.NET 10 ASP.NET Core** | Latest LTS-bound runtime, minimal hosting model, built-in DI, validation, and Swagger integration |
| **Google Sheets API v4** | Official Google client library for .NET; strongly typed, handles OAuth/JWT internally |
| **Google Drive API v3** | Same service account credentials as Sheets; `Files.Create` with `parents` uploads directly to a shared folder |
| **Google Service Account** | Server-to-server auth with no user login flow required; credentials are a JSON file scoped to only Sheets + Drive |
| **Swashbuckle / OpenAPI 3** | Auto-generates interactive Swagger UI from controller attributes; zero extra maintenance |
| **DataAnnotations** | Declarative input validation on DTOs — keeps controllers clean and validation rules colocated with the model |
| **Docker (multi-stage)** | Separates build environment from runtime image; final image is minimal (`aspnet:10.0`, not `sdk:10.0`) |
| **Render.com** | Free-tier Docker hosting with environment variable support; no credit card required for basic deployment |

---

## Project Structure

```
car-api/
├── Controllers/
│   └── CarsController.cs       # HTTP layer — maps routes to service calls, handles errors
├── Models/
│   └── Car.cs                  # Domain model — the internal representation of a car
├── DTOs/
│   └── CarDto.cs               # CarCreateRequest (input + validation) and CarResponse (output shape)
├── Services/
│   ├── IGoogleSheetsService.cs # Interface — keeps controller decoupled from implementation
│   ├── GoogleSheetsService.cs  # Reads/writes rows in Google Sheets
│   ├── IGoogleDriveService.cs  # Interface
│   └── GoogleDriveService.cs  # Uploads files to a Google Drive folder
├── Program.cs                  # App bootstrap: DI registration, Swagger config, CORS, middleware pipeline
├── CarApi.csproj               # .NET 10 project — package references (Google APIs, Swashbuckle)
├── Dockerfile                  # Multi-stage build: SDK image for build, ASP.NET image for runtime
├── docker-compose.yml          # Local Docker Compose with environment variable wiring
├── test-api.sh                 # Shell script: 7 POST calls + 4 GET queries to demo the full flow
├── appsettings.json            # Local config (credentials inline for dev — move to env vars for prod)
├── GOOGLE_CLOUD_SETUP.md       # Step-by-step GCP project + service account setup
├── DEPLOY_RENDER.md            # Step-by-step Render.com deployment guide
└── DEMO_SCRIPT.md              # Guided < 1-minute screen recording script
```

### Why interfaces for services?

`IGoogleSheetsService` and `IGoogleDriveService` exist so that:
- The controller depends on the **abstraction**, not the concrete Google client
- In tests, you can inject a mock/fake without touching production code
- The DI container (`Program.cs`) is the only place that knows which concrete class to use

---

## Data Model

The `Car` domain model (`Models/Car.cs`) has 5 user-supplied fields and 3 system fields:

| Field | Type | Source | Description |
|---|---|---|---|
| `id` | string (GUID) | Auto-generated | `Guid.NewGuid().ToString()` on creation |
| `make` | string | User input | Manufacturer — required, max 50 chars |
| `model` | string | User input | Model name — required, max 50 chars |
| `year` | int | User input | Model year — required, range 1900–2030 |
| `price` | decimal | User input | Price in USD — required, range 0–1,000,000 |
| `color` | string | User input | Exterior color — required, max 30 chars |
| `imageUrl` | string? | User input or Drive | Direct URL string, or Drive `webViewLink` after upload |
| `createdAt` | datetime | Auto-generated | `DateTime.UtcNow` at time of POST |

### Why two separate classes (Car vs CarCreateRequest)?

- `Car` is the **internal domain model** — includes `Id`, `CreatedAt`, `ImageUrl`; used within the service layer
- `CarCreateRequest` is the **public API contract** — only the 5 fields a client needs to send; has `[Required]` and `[Range]` attributes for validation
- `CarResponse` is the **output shape** — same as `Car` but as a DTO so the internal model is never directly serialized

This separation means you can change internal logic without breaking the public API contract.

---

## API Endpoints

### POST /api/cars

Adds a new car record to Google Sheets.

**Request** — `multipart/form-data`:

| Field | Required | Type | Notes |
|---|---|---|---|
| `make` | Yes | string | Max 50 chars |
| `model` | Yes | string | Max 50 chars |
| `year` | Yes | int | 1900–2030 |
| `price` | Yes | decimal | 0–1,000,000 |
| `color` | Yes | string | Max 30 chars |
| `imageUrl` | No | string | Direct URL to any image |
| `image` | No | file | Binary file upload (requires Google Workspace Shared Drive) |

**Response — `201 Created`:**
```json
{
  "id": "91ea7611-30d4-4e2b-94a5-ff0eef108acb",
  "make": "Toyota",
  "model": "Camry",
  "year": 2023,
  "price": 28000,
  "color": "White",
  "imageUrl": null,
  "createdAt": "2026-04-15T18:42:42.931441Z"
}
```

**Response — `400 Bad Request`** (validation failure):
```json
{
  "errors": {
    "Year": ["The field Year must be between 1900 and 2030."]
  }
}
```

---

### GET /api/cars

Returns car records from Google Sheets, filtered using LINQ.

**Query Parameters** (all optional):

| Param | Type | Example | Behaviour |
|---|---|---|---|
| `make` | string | `make=Toyota` | Case-insensitive exact match |
| `color` | string | `color=Red` | Case-insensitive exact match |
| `minYear` | int | `minYear=2020` | Inclusive lower bound on year |
| `maxYear` | int | `maxYear=2024` | Inclusive upper bound on year |
| `minPrice` | decimal | `minPrice=20000` | Inclusive lower bound on price |
| `maxPrice` | decimal | `maxPrice=50000` | Inclusive upper bound on price |
| `limit` | int | `limit=5` | Max records returned (default: 100) |

**Example requests:**
```bash
# All Toyotas under $30k
GET /api/cars?make=Toyota&maxPrice=30000

# 2022–2023 models, first 3 results
GET /api/cars?minYear=2022&maxYear=2023&limit=3

# All red cars
GET /api/cars?color=Red
```

---

### DELETE /api/cars

Clears all car rows from the sheet while keeping the header row (row 1) intact.

**Response — `200 OK`:**
```json
{ "message": "All car records cleared" }
```

---

### GET /health

```json
{ "status": "healthy", "timestamp": "2026-04-15T18:42:42Z" }
```

Used by Render.com and Docker health checks to confirm the process is running.

---

## How LINQ Filtering Works

The GET endpoint uses LINQ (Language Integrated Query) to filter in memory. Here is the exact flow:

```csharp
// 1. Fetch all rows from Google Sheets as a List<Car>
var cars = await _sheetsService.GetAllCarsAsync();

// 2. Convert to IQueryable so LINQ expressions can be chained
var query = cars.AsQueryable();

// 3. Each filter is only applied if the parameter was provided
if (!string.IsNullOrWhiteSpace(make))
    query = query.Where(c => c.Make.Equals(make, StringComparison.OrdinalIgnoreCase));

if (maxPrice.HasValue)
    query = query.Where(c => c.Price <= maxPrice.Value);

// 4. Cap the result set
var results = query.Take(limit).ToList();
```

**Why fetch all rows and filter in memory?**

Google Sheets does not have a query engine — you cannot send a `WHERE` clause to the Sheets API. The only option is to read all rows and filter them yourself. This is a standard pattern when using Sheets as a lightweight data store, and works well for datasets up to roughly 10,000 rows.

**Why `AsQueryable()` instead of just `Where()` on the list?**

`AsQueryable()` allows the filters to be composed as an expression tree and evaluated in a single pass. The result is the same as chaining `.Where()` directly, but it signals the intent clearly and would allow the query to be handed off to a real database provider (Entity Framework, for example) with minimal code change in future.

---

## How Google Authentication Works

The API uses a **Google Service Account** — a robot identity created in Google Cloud Console with no human login.

### Flow on startup

1. `GoogleSheetsService` reads the `CredentialsJson` string from configuration
2. `GoogleCredential.FromJson(credentialJson)` parses the JSON and creates a credential object
3. `.CreateScoped(SheetsService.Scope.Spreadsheets)` restricts the credential to only the Sheets API
4. The Google client library internally signs a JWT using the service account's private key and exchanges it for a short-lived OAuth 2.0 access token
5. All API calls attach this token in the `Authorization: Bearer ...` header automatically

### Why a Service Account and not OAuth?

OAuth requires a human to log in via a browser and grant permissions. A service account is designed for server-to-server calls where there is no human in the loop. Since this API runs as a background service, a service account is the correct choice.

### Why registered as a Singleton?

```csharp
builder.Services.AddSingleton<IGoogleSheetsService, GoogleSheetsService>();
```

The Google API client (`SheetsService`, `DriveService`) is expensive to construct — it parses credentials, sets up an HTTP client, and handles token refresh. Creating it once per application lifetime (Singleton) avoids paying that cost on every request. The client is thread-safe, so sharing it across concurrent requests is safe.

---

## Google Sheets Schema

The API reads/writes the sheet named **`Cars`** with this column layout:

| Column | A | B | C | D | E | F | G | H |
|---|---|---|---|---|---|---|---|---|
| Header | Id | Make | Model | Year | Price | Color | ImageUrl | CreatedAt |

**Row 1** must contain these headers exactly. Data starts from **row 2**.

### Why the append range is `Cars!A:G` but we write 8 columns

The Google Sheets `Append` API uses the range parameter to **detect where the existing table ends**, not to specify which columns to write. When the range is set to `Cars!A:H` (all 8 columns), the API interprets the last column of the range as an "anchor" and shifts the next write to column I — a known quirk of the Sheets Append API.

The fix: use `Cars!A:G` as the append range (7 columns). The 8th value (`CreatedAt`) is still written correctly because Sheets allows values to spill one column beyond the specified range. For reading, `Cars!A:H` is used to explicitly include all 8 columns.

---

## Design Decisions & Trade-offs

| Decision | Choice | Why |
|---|---|---|
| Data store | Google Sheets | No DB infrastructure needed; data is human-readable and shareable |
| Auth | Service Account | No user login flow; correct pattern for server-to-server API calls |
| DI lifetime | Singleton | Google API clients are expensive to construct and are thread-safe |
| Filtering strategy | In-memory LINQ | Sheets has no query engine; fetch-all + filter is the only option |
| Input validation | DataAnnotations | Built into ASP.NET; runs before controller code; keeps validation colocated with the DTO |
| Error handling | try/catch in controller | Wraps external API calls (Sheets, Drive) which can fail with network/auth errors |
| POST body format | multipart/form-data | Supports both text fields and optional file upload in a single request |
| Swagger in production | Opt-in via `ENABLE_SWAGGER=true` env var | Swagger is off by default in production; explicitly enabled for public demo hosting |

---

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Google Cloud project with Sheets API and Drive API enabled
- Service account JSON credentials
- A Google Sheet with the `Cars` tab and correct headers (see [Google Cloud Setup](#google-cloud-setup))

### Run Locally

```bash
git clone https://github.com/yourusername/car-api.git
cd car-api

# Credentials are read from appsettings.json in development
dotnet run --project CarApi.csproj

# Swagger UI: http://localhost:5000
# Health check: http://localhost:5000/health
```

### Run with Docker

```bash
docker build -t car-api .

docker run -p 8080:8080 \
  -e ENABLE_SWAGGER=true \
  -e GoogleSheets__SpreadsheetId=<your-spreadsheet-id> \
  -e "GoogleSheets__CredentialsJson={...service account json...}" \
  -e GoogleDrive__FolderId=<your-folder-id> \
  -e "GoogleDrive__CredentialsJson={...service account json...}" \
  car-api

# Swagger UI: http://localhost:8080
```

---

## Configuration

| appsettings.json Key | Environment Variable | Description |
|---|---|---|
| `GoogleSheets:SpreadsheetId` | `GoogleSheets__SpreadsheetId` | Spreadsheet ID from the URL (`/d/{ID}/edit`) |
| `GoogleSheets:CredentialsJson` | `GoogleSheets__CredentialsJson` | Full service account JSON as a single-line string |
| `GoogleDrive:FolderId` | `GoogleDrive__FolderId` | Drive folder ID from the URL (`/folders/{ID}`) |
| `GoogleDrive:CredentialsJson` | `GoogleDrive__CredentialsJson` | Full service account JSON as a single-line string |
| `ENABLE_SWAGGER` | `ENABLE_SWAGGER` | Set to `true` to expose Swagger UI in production |

ASP.NET Core maps environment variables with `__` to nested JSON keys (e.g. `GoogleSheets__SpreadsheetId` → `GoogleSheets:SpreadsheetId`). This is a built-in convention — no extra code required.

> **Security:** The `appsettings.json` in this repo contains credentials for local development. Before pushing to a public repository, remove the credentials and use environment variables or a secrets manager (e.g. Azure Key Vault, AWS Secrets Manager, Render's environment variable dashboard).

---

## Google Cloud Setup

Full guide: [GOOGLE_CLOUD_SETUP.md](./GOOGLE_CLOUD_SETUP.md)

**Summary:**

1. Go to [console.cloud.google.com](https://console.cloud.google.com) → create a new project
2. Enable **Google Sheets API** and **Google Drive API** under APIs & Services → Library
3. Go to APIs & Services → Credentials → Create Credentials → **Service Account**
4. On the service account page → Keys tab → Add Key → JSON → download the file
5. Create a Google Sheet → rename the first tab to `Cars` → add headers in row 1:
   `Id | Make | Model | Year | Price | Color | ImageUrl | CreatedAt`
6. Share the sheet with the service account email (from the JSON: `client_email` field) as **Editor**
7. Copy the Spreadsheet ID from the URL and the folder ID from your Drive folder URL

---

## Deployment to Render.com

Full guide: [DEPLOY_RENDER.md](./DEPLOY_RENDER.md)

**Summary:**

1. Push this repo to GitHub
2. Go to [render.com](https://render.com) → New → Web Service → connect your GitHub repo
3. Set Runtime to **Docker**, Instance Type to **Free**
4. Add environment variables:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ENABLE_SWAGGER` = `true`
   - `GoogleSheets__SpreadsheetId` = your spreadsheet ID
   - `GoogleSheets__CredentialsJson` = your service account JSON (paste the full JSON)
   - `GoogleDrive__FolderId` = your Drive folder ID
   - `GoogleDrive__CredentialsJson` = same service account JSON
5. Click **Create Web Service** — Render builds the Docker image and deploys it
6. Swagger UI is accessible at the root URL of your Render service

> Free tier note: The service spins down after 15 minutes of inactivity. The first request after spin-down takes ~30 seconds (cold start).

---

## Testing

### Run the full test suite (7 cars + 4 queries):

```bash
chmod +x test-api.sh
./test-api.sh
```

Creates: Toyota Camry, Honda Civic, Toyota Corolla, Ford Mustang, BMW X5, Honda Accord, Toyota RAV4

Runs queries:
1. All cars
2. All Toyotas (`make=Toyota`)
3. Cars under $30k (`maxPrice=30000`)
4. Toyotas under $30k (`make=Toyota&maxPrice=30000`)

### Manual curl examples:

```bash
BASE=http://localhost:5000

# Create a car
curl -X POST $BASE/api/cars \
  -F "make=Toyota" -F "model=Camry" \
  -F "year=2023" -F "price=28000" -F "color=White"

# Create a car with an image URL
curl -X POST $BASE/api/cars \
  -F "make=Porsche" -F "model=911" -F "year=2024" \
  -F "price=120000" -F "color=Yellow" \
  -F "imageUrl=https://example.com/porsche.jpg"

# Get all Toyotas under $30k
curl "$BASE/api/cars?make=Toyota&maxPrice=30000"

# Clear all records
curl -X DELETE $BASE/api/cars

# Health check
curl $BASE/health
```

---

## Known Limitations

| Limitation | Detail |
|---|---|
| **No pagination cursor** | `limit` caps results but there is no offset/page parameter. For large datasets, all rows are still fetched from Sheets before truncation. |
| **No update or single-record delete** | Only append (POST), read (GET), and full clear (DELETE) are implemented. Updating a specific row in Sheets requires knowing the row number, which adds complexity not needed for this scope. |
| **In-memory filtering at scale** | All rows are fetched from Sheets on every GET. This is efficient up to ~10k rows; beyond that, a proper database would be needed. |
| **No authentication on the API itself** | The API endpoints are public. A production system would add API key validation or JWT bearer tokens on the controller. |
| **Google Drive file upload requires Shared Drive** | Service accounts have no personal Drive storage quota. File uploads work when the target folder is in a Google Workspace Shared Drive. For personal accounts, passing an `imageUrl` string directly is the supported path. |
| **Credentials in appsettings.json** | Included for easy local setup. Must be moved to environment variables before pushing to a public repository. |
