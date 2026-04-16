# Demo Recording Script (< 1 minute)

## Setup
- Open your deployed Swagger UI: `https://car-api-xxxx.onrender.com`
- Have screen recording ready (QuickTime, OBS, or Loom)
- Close unnecessary tabs/apps

## Script (45-55 seconds)

### [0:00-0:05] Swagger UI Overview
- Show the Swagger UI homepage
- Point out: "This is our publicly hosted Car API with 2 endpoints"

### [0:05-0:20] POST Car Records
- Expand the POST `/api/cars` endpoint
- Click "Try it out"
- Fill in: Make=Toyota, Model=Camry, Year=2023, Price=28000, Color=White
- Click "Execute"
- Show: "201 Created - Car added to Google Sheets"

### [0:20-0:35] Query with Filters
- Expand the GET `/api/cars` endpoint
- Click "Try it out"
- Add filter: `make=Toyota`, `maxPrice=30000`
- Click "Execute"
- Show: "Filtered results using LINQ - returns only matching cars"

### [0:35-0:45] Show Google Sheets
- Switch to Google Sheets tab
- Show the populated rows
- Say: "All records stored in Google Sheets"

### [0:45-0:55] Wrap Up
- "That's it - .NET 10 API, Google Sheets integration, publicly hosted on Render"

## Quick Command Alternative

If you prefer terminal demo:

```bash
# Record terminal with QuickTime
# Run these commands:

# 1. POST a car
curl -X POST https://car-api-xxxx.onrender.com/api/cars \
  -F "make=Toyota" -F "model=Camry" -F "year=2023" \
  -F "price=28000" -F "color=White" | jq .

# 2. Query with filter
curl "https://car-api-xxxx.onrender.com/api/cars?make=Toyota&maxPrice=30000" | jq .
```

## Tips
- Use browser zoom: 110% for better visibility
- Pre-fill some test data before recording
- Keep mouse movements smooth
- Record in 1080p minimum
