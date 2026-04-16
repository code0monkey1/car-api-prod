# Hiring Manager Presentation Script (2-3 minutes)

**Target audience:** Engineering managers, hiring leads  
**Tone:** Professional, showcase technical depth + pragmatism  
**Video format:** Screen recording with voiceover  
**Total length:** 2-3 minutes

---

## [0:00-0:15] Problem & Solution

**VISUAL:** Show the Car API GitHub repo README  
**VOICEOVER:**

"I built a REST API called **Car API** — a production-ready, fully hosted backend service that demonstrates modern cloud architecture and full-stack engineering.

The project uses **.NET 10 with ASP.NET Core** as the API framework, and instead of a traditional database, it uses **Google Sheets as the data store** with **Google Drive for file hosting**. This might sound unconventional, but it's a deliberate architectural choice that trades off scale for simplicity and observability."

---

## [0:15-0:40] Technical Architecture

**VISUAL:** Diagram or GitHub code overview (Controllers → Services → Google APIs)  
**VOICEOVER:**

"Here's the architecture:

**Layer 1 — HTTP Layer:** ASP.NET Core controllers handle incoming requests. Input validation happens via DataAnnotations — no invalid data gets through.

**Layer 2 — Business Logic:** Two service interfaces — one for Google Sheets, one for Google Drive. They're dependency-injected as singletons because Google API clients are expensive to construct and thread-safe.

**Layer 3 — Google APIs:** We authenticate via a service account — a robot identity with scoped permissions. The JWT is signed server-side; no user login flow needed.

**Data flow:**  
- POST request → Validate input → Create Car object with GUID and timestamp → Append to Google Sheets → Return 201 Created
- GET request → Fetch all rows from Sheets → Filter in-memory with LINQ → Return filtered results

**The key insight:** Google Sheets has no query engine, so we fetch-all + filter-in-memory. This works for datasets under ~10k rows and keeps infrastructure costs at zero."

---

## [0:40-1:00] Live Demo

**VISUAL:** Navigate to deployed Swagger UI at `https://car-api-xxxx.onrender.com`  
**VOICEOVER:**

"Let me show you it in action.

This is the **Swagger UI** — auto-generated from OpenAPI specs. Every endpoint is documented and testable.

Let's create a car record."

**ACTION:** Expand POST `/api/cars` → Click "Try it out" → Fill in form:
- Make: Toyota
- Model: Camry
- Year: 2023
- Price: 28000
- Color: White

Click Execute → Show 201 Created response with ID, timestamp, etc.

**VOICEOVER:**

"One request — the car is now in Google Sheets. Notice the response includes the auto-generated ID and CreatedAt timestamp."

**ACTION:** Switch to GET `/api/cars` → Click "Try it out" → Add filters:
- make=Toyota
- maxPrice=30000

Execute → Show filtered results.

**VOICEOVER:**

"LINQ filtering happens in-memory. This request fetches all cars, filters by make and price, and returns matches. For a real high-volume API, you'd use a database with query pushdown, but for this use case — a demo or lightweight app — this is practical and transparent."

---

## [1:00-1:30] Engineering Decisions & Trade-offs

**VISUAL:** README section on "Design Decisions & Trade-offs" or a summary slide  
**VOICEOVER:**

"Let me highlight the engineering decisions I made:

**1. .NET 10 over Node/Python:**  
I chose .NET because it has the best-in-class DI, validation, and middleware ecosystem. Built-in Swagger generation saves hours. Strong typing catches bugs at compile time.

**2. Google Sheets over PostgreSQL/MongoDB:**  
Trade-off analysis: 
- **Pro:** Zero infrastructure. Sheets data is human-readable and shareable. No database scaling, backups, or patching.
- **Con:** No complex queries. Single-threaded. Won't scale to millions of records.
- **Verdict:** Right choice for a demo, a non-critical feature, or a prototype. Wrong choice for a production OLTP system.

**3. Service Account auth over OAuth:**  
Server-to-server calls don't need user login. Service accounts are simpler, more secure for backend APIs.

**4. Singleton DI lifetime for Google clients:**  
Google API clients are thread-safe but expensive to instantiate. Singleton means we pay that cost once, not once per request. Saves ~10-50ms per request.

**5. DataAnnotations for validation:**  
Runs before controller code. Declarative, testable, colocated with the model. Keeps controllers thin."

---

## [1:30-2:00] Deployment & Observability

**VISUAL:** Show Render.com dashboard or GitHub Actions logs  
**VOICEOVER:**

"This API is **publicly deployed on Render.com** — a free-tier container host. The Dockerfile is multi-stage:
- Build stage uses the .NET SDK (large, for compilation)
- Runtime stage uses the lean ASP.NET image (small, for execution)

Final image is ~375MB — small enough for fast cold starts.

All credentials are **environment variables** — never in code. The GitHub repo is public, but no secrets are exposed.

Health checks run every 10 seconds. If the service crashes, Render auto-restarts it.

For a real production system, I'd add:
- Request logging + tracing (Application Insights, Datadog)
- Rate limiting + authentication
- Monitoring dashboards
- Database instead of Sheets

But for a demo, this is battle-tested and reliable."

---

## [2:00-2:30] Wrap-up & What This Shows

**VISUAL:** Back to repo homepage  
**VOICEOVER:**

"Here's what this project demonstrates:

✅ **Full-stack ownership:** Infrastructure, API, data layer, documentation.  
✅ **Pragmatic trade-offs:** Chose tools that were appropriate for the problem, not hype.  
✅ **Production-ready code:** Error handling, validation, logging, Swagger docs, Docker.  
✅ **Cloud architecture:** Service accounts, environment variables, container deployment.  
✅ **Communication:** Detailed README, architecture diagrams, deployment guides.

The code is small (600 lines) but architecturally sound. No over-engineering. Every class has a reason.

**Thanks for watching. Questions?**"

---

## Recording Tips

### Tools (Free):
- **MacOS:** QuickTime (built-in) or ScreenFlow
- **Windows:** OBS Studio or ScreenFlow alternative
- **Any:** Loom (free tier, cloud hosted)

### Recording checklist:
- Zoom browser to 110-125% for readability
- 1080p minimum resolution
- Mute notifications
- Pre-populate some test data in Sheets
- Do a dry run first
- Keep voice clear, moderate pace (not rushed)
- Total time: 2-3 min (hiring managers have limited attention)

### Voiceover recording:
- Use free tools: Audacity (free audio editor), or record directly in Loom
- Reduce background noise in post-processing
- Keep script bullet points visible while recording

### Post-production:
- Trim intro/outro if needed
- Add captions for accessibility (YouTube auto-captions are free)
- Optional: add background music (royalty-free from Pexels Music, YouTube Audio Library)

---

## Upload Services (Free)

| Service | Best for | Free limits | Notes |
|---------|----------|-------------|-------|
| **YouTube** (unlisted) | Long-form (any length) | Unlimited storage | Most reliable, best sharing, auto-captions |
| **Loom** | Quick demos (<5 min) | 25 recordings/month | Built-in editing, webcam overlay, instant link |
| **Vimeo** | Professional showcase | 500MB/week | Clean player, password protection, analytics |
| **Google Drive** | Private sharing | 15GB free | Simple, integrates with Gmail |
| **Wistia** | Hiring/talent (up to 3 videos) | 3 free videos | Embedded player, engagement tracking |

### **Recommendation:**
- **YouTube (unlisted):** Best choice. Upload, keep unlisted, send link to hiring manager. No expiry, unlimited length, built-in captions.
- **Loom:** If < 5 min and you want fastest turnaround (records + uploads in seconds).

---

## Sample Sending Email

```
Hi [Manager Name],

I wanted to share a project I built: a production REST API using .NET 10, Google Sheets integration, and cloud deployment.

[Video Link - YouTube Unlisted or Loom]

The code is open source: https://github.com/code0monkey1/car-api-prod

The live API is here: https://car-api-xxxx.onrender.com (Swagger UI at the root)

In the video, I walk through:
- Architecture & design decisions
- Live demo of the API
- Deployment strategy
- Trade-offs I made and why

Happy to discuss any questions!

[Your name]
```
