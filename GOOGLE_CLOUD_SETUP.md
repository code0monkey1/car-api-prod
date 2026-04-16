# Google Cloud Setup Guide

## Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "New Project" (or select existing)
3. Name it (e.g., "Car API")
4. Click "Create"

## Step 2: Enable APIs

1. In the Cloud Console, go to **APIs & Services > Library**
2. Search for and enable:
   - **Google Sheets API**
   - **Google Drive API**

## Step 3: Create Service Account

1. Go to **APIs & Services > Credentials**
2. Click **Create Credentials > Service Account**
3. Give it a name (e.g., "car-api-service")
4. Click "Create and Continue"
5. Assign role: **Project > Editor** (or specific Sheets/Drive roles)
6. Click "Continue" then "Done"

## Step 4: Download Credentials

1. Find your service account in the list
2. Click the email address → **Keys** tab
3. Click **Add Key > Create new key**
4. Select **JSON** format
5. Download and save the file (e.g., `credentials.json`)

## Step 5: Create Google Sheet

1. Create a new Google Sheet
2. Name it (e.g., "Car Database")
3. In the first row, add headers:
   ```
   A1: Id
   B1: Make
   C1: Model
   D1: Year
   E1: Price
   F1: Color
   G1: ImageUrl
   H1: CreatedAt
   ```
4. Name the sheet tab "Cars" (default is "Sheet1" - rename it)
5. Copy the **Spreadsheet ID** from the URL:
   - URL: `https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/edit`
   - Copy the part between `/d/` and `/edit`

## Step 6: Create Google Drive Folder (Optional)

1. Create a folder in Google Drive (e.g., "Car Images")
2. Copy the **Folder ID** from the URL:
   - URL: `https://drive.google.com/drive/folders/{FOLDER_ID}`
   - Copy the part after `/folders/`

## Step 7: Share with Service Account

1. Open the Google Sheet
2. Click **Share** (top right)
3. Enter the service account email (from your JSON file: `client_email` field)
4. Give **Editor** access
5. Repeat for the Drive folder

## Step 8: Prepare Environment Variables

For local development:
```bash
export GOOGLE_SHEETS_SPREADSHEET_ID="your-spreadsheet-id"
export GOOGLE_SHEETS_CREDENTIALS_JSON='{"type":"service_account",...}'
export GOOGLE_DRIVE_FOLDER_ID="your-folder-id"
export GOOGLE_DRIVE_CREDENTIALS_JSON='{"type":"service_account",...}'
```

For Render.com deployment, add these as environment variables in the dashboard.

## Clean Shared Google Drive Folder

1. Go to your shared Drive folder
2. Remove any existing files not related to this project
3. Ensure only the "Car Images" folder and related files exist
4. This keeps the workspace clean for the demo
