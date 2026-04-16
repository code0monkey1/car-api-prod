# Deploy to Render.com

## Prerequisites
- Docker installed locally
- Render.com account (free tier available)
- Google Cloud setup completed (see GOOGLE_CLOUD_SETUP.md)

## Step 1: Build & Test Locally

```bash
cd ~/Documents/car-api

# Build the Docker image
docker build -t car-api .

# Run locally
docker run -p 8080:8080 \
  -e GOOGLE_SHEETS_SPREADSHEET_ID=your-id \
  -e GOOGLE_SHEETS_CREDENTIALS_JSON='{"type":"service_account"...}' \
  -e GOOGLE_DRIVE_FOLDER_ID=your-folder-id \
  -e GOOGLE_DRIVE_CREDENTIALS_JSON='{"type":"service_account"...}' \
  car-api
```

Access Swagger at: `http://localhost:8080`

## Step 2: Push to GitHub

1. Create a GitHub repository
2. Initialize git in the car-api folder:
```bash
cd ~/Documents/car-api
git init
git add .
git commit -m "Initial commit: Car API with Google Sheets/Drive"
git remote add origin git@github.com:yourusername/car-api.git
git push -u origin main
```

## Step 3: Deploy on Render.com

1. Go to [Render.com](https://render.com) and log in
2. Click **New + > Web Service**
3. Connect your GitHub repository
4. Configure:
   - **Name**: car-api
   - **Region**: Choose closest to you
   - **Branch**: main
   - **Root Directory**: Leave blank
   - **Runtime**: Docker
   - **Instance Type**: Free

5. Add Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ENABLE_SWAGGER=true
   GoogleSheets__SpreadsheetId=<your-spreadsheet-id>
   GoogleSheets__CredentialsJson=<paste-full-JSON>
   GoogleDrive__FolderId=<your-folder-id>
   GoogleDrive__CredentialsJson=<paste-full-JSON>
   ```

6. Click **Create Web Service**

## Step 4: Access Your API

- Your API will be at: `https://car-api-xxxx.onrender.com`
- Swagger UI: Same URL (root)
- Health Check: `https://car-api-xxxx.onrender.com/health`

## Step 5: Test Deployment

```bash
export CAR_API_URL=https://car-api-xxxx.onrender.com
chmod +x test-api.sh
./test-api.sh
```

## Notes
- Free tier spins down after 15 minutes of inactivity (cold start ~30s)
- Swagger is enabled in production via `ENABLE_SWAGGER=true`
- All credentials are stored securely as environment variables
