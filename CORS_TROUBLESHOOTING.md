# CORS Error Troubleshooting Guide

## Issue: CORS (Cross-Origin Resource Sharing) Error

If you're getting a CORS error when the frontend tries to connect to the backend, this guide will help.

## What is CORS?

CORS is a security feature that prevents web pages from making requests to a different domain/port than the one that served the web page. When your frontend (running on one port) tries to talk to your backend (running on a different port), the browser blocks it unless the backend explicitly allows it.

## Common CORS Error Messages

- `Access to fetch at 'https://localhost:7001/api/...' from origin 'https://localhost:7287' has been blocked by CORS policy`
- `No 'Access-Control-Allow-Origin' header is present`
- `CORS policy: No 'Access-Control-Allow-Credentials' header`

## Solution

The CORS policy has been configured in `src/Backend/DiscordApp.API/Program.cs` to allow requests from the frontend.

### Current Configuration

**Backend ports:**
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:7001`

**Frontend ports:**
- HTTP: `http://localhost:5180`
- HTTPS: `https://localhost:7287`

**Allowed origins in CORS policy:**
```csharp
policy.WithOrigins(
    "http://localhost:5180",   // Frontend HTTP
    "https://localhost:7287",  // Frontend HTTPS
    "http://localhost:5000",   // Additional for compatibility
    "https://localhost:5001", 
    "http://localhost:5173")
```

## If You Still Get CORS Errors

### 1. Check Your Ports

Make sure both backend and frontend are running on the expected ports:

**Backend:**
```bash
cd src/Backend/DiscordApp.API
dotnet run
# Should show: Now listening on: https://localhost:7001
```

**Frontend:**
```bash
cd src/Frontend/DiscordApp.Client
dotnet run
# Should show: Now listening on: https://localhost:7287
```

### 2. Verify Frontend API Configuration

Check `src/Frontend/DiscordApp.Client/wwwroot/appsettings.json`:
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

This should match the backend HTTPS port.

### 3. Clear Browser Cache

CORS headers can be cached by browsers. Try:
1. Close all browser tabs
2. Clear browser cache
3. Restart the browser
4. Try again

### 4. Use Browser Developer Tools

Open the browser console (F12) and check:
1. **Console tab:** Look for CORS error messages
2. **Network tab:** Check the request headers and response headers
3. Look for the `Access-Control-Allow-Origin` header in responses

### 5. Check If CORS Is Applied

The CORS policy must be applied in the middleware pipeline. Verify in `Program.cs`:

```csharp
app.UseCors();  // Must be before UseAuthentication and UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
```

### 6. For SignalR Connections

SignalR also needs CORS. Make sure:
1. SignalR hubs are mapped after `app.UseCors()`
2. The SignalR client includes credentials:
```csharp
.WithUrl($"{_baseUrl}/hubs/chat", options =>
{
    options.AccessTokenProvider = () => Task.FromResult(token)!;
})
```

## Custom Ports

If you're using different ports, update the CORS policy:

1. Open `src/Backend/DiscordApp.API/Program.cs`
2. Find the CORS configuration (around line 93)
3. Add your custom origin:
```csharp
policy.WithOrigins(
    "http://localhost:YOUR_PORT",
    "https://localhost:YOUR_HTTPS_PORT",
    // ... existing origins
)
```
4. Rebuild and restart the backend

## Production Considerations

For production:
1. **Never use** `AllowAnyOrigin()` - always specify exact origins
2. Update CORS policy with your production domain
3. Consider environment-specific configuration:
```csharp
var allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:5180", "https://localhost:7287" }
    : new[] { "https://yourdomain.com" };

policy.WithOrigins(allowedOrigins)
```

## Testing the Fix

1. Start the backend:
```bash
cd src/Backend/DiscordApp.API
dotnet run
```

2. Start the frontend:
```bash
cd src/Frontend/DiscordApp.Client
dotnet run
```

3. Open the frontend URL in your browser (e.g., `https://localhost:7287`)

4. Try to login or register - the API calls should now work without CORS errors

## Still Having Issues?

If you're still experiencing CORS errors:

1. Check that both applications are running
2. Verify the ports match the configuration
3. Look at the browser console for the exact error message
4. Check the Network tab to see what origin is being sent
5. Ensure no proxy or firewall is interfering
6. Try using HTTP instead of HTTPS for testing (simpler setup)

## Related Files

- Backend CORS config: `src/Backend/DiscordApp.API/Program.cs`
- Frontend API config: `src/Frontend/DiscordApp.Client/wwwroot/appsettings.json`
- Frontend service setup: `src/Frontend/DiscordApp.Client/Program.cs`
