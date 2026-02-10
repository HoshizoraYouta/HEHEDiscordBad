# Quick Start - Correct URLs

## ❌ WRONG (Causes ERR_SSL_PROTOCOL_ERROR)
```
https://localhost:5118/api/Auth/register
```
**Why it fails:** Port 5118 is HTTP-only, not HTTPS

## ✅ CORRECT - Use These URLs

### Option 1: HTTPS (Recommended)
```
https://localhost:7001/api/Auth/register
```
- Secure connection
- Requires trusted certificate: `dotnet dev-certs https --trust`
- Swagger: `https://localhost:7001/swagger`

### Option 2: HTTP (Development Only)
```
http://localhost:5001/api/Auth/register
```
- No SSL required
- Faster for local testing
- Swagger: `http://localhost:5001/swagger`

## First Time Setup

Before using HTTPS, run this command once:

```bash
dotnet dev-certs https --trust
```

This will:
- Generate a development SSL certificate
- Trust it in your system
- Allow HTTPS connections to localhost

## Summary

| Service | Protocol | Port | URL |
|---------|----------|------|-----|
| **Backend API** | HTTPS ✅ | 7001 | https://localhost:7001 |
| Backend API | HTTP  ⚠️ | 5001 | http://localhost:5001 |
| **Frontend** | HTTPS ✅ | 7287 | https://localhost:7287 |
| Frontend | HTTP  ⚠️ | 5180 | http://localhost:5180 |

## Common Issues

- **SSL errors?** See [SSL_SETUP.md](SSL_SETUP.md)
- **CORS errors?** See [CORS_TROUBLESHOOTING.md](CORS_TROUBLESHOOTING.md)

For more details, see [SSL_SETUP.md](SSL_SETUP.md)
