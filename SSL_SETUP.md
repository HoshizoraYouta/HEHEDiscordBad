# SSL/HTTPS Development Setup Guide

## Issue: ERR_SSL_PROTOCOL_ERROR

If you're getting an SSL protocol error when accessing the API, follow these steps:

## Quick Fix

### 1. Trust the .NET Development Certificate

Run this command to trust the development certificate:

```bash
dotnet dev-certs https --trust
```

**On Windows:** This will prompt you to trust the certificate.
**On macOS:** This will prompt for your password.
**On Linux:** You may need to manually trust the certificate in your browser.

### 2. Verify Certificate Installation

```bash
dotnet dev-certs https --check
```

If the certificate is not valid, clean and recreate it:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 3. Use the Correct Ports

The application is configured with the following ports:

- **HTTPS (Secure):** `https://localhost:7001`
- **HTTP (Not Secure):** `http://localhost:5001`

**Always use the HTTPS URL for production-like testing:**
```
https://localhost:7001/api/Auth/register
```

## Alternative: Run Without HTTPS (Development Only)

If you want to run without HTTPS for local development:

1. Use the HTTP profile:
```bash
cd src/Backend/DiscordApp.API
dotnet run --launch-profile http
```

2. Access the API via HTTP:
```
http://localhost:5001/api/Auth/register
```

## Browser-Specific Solutions

### Chrome/Edge
1. Type `chrome://flags/#allow-insecure-localhost` in the address bar
2. Enable "Allow invalid certificates for resources loaded from localhost"
3. Restart the browser

### Firefox
1. When you see the security warning, click "Advanced"
2. Click "Accept the Risk and Continue"

## Troubleshooting

### Certificate Already Exists Error
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Still Getting Errors?
1. Close all browser windows
2. Clear browser cache and SSL state
3. Restart your development server
4. Try accessing the API again

### Port Already in Use
If you get "Address already in use" errors:

**Windows:**
```powershell
netstat -ano | findstr :7001
taskkill /PID <PID> /F
```

**Linux/Mac:**
```bash
lsof -i :7001
kill -9 <PID>
```

## Updated Configuration

The `launchSettings.json` has been updated to use standard ports:
- HTTPS: 7001
- HTTP: 5001

This matches the documentation in README.md and ensures consistency across the application.
