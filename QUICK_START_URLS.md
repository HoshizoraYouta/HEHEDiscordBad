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

| Protocol | Port | URL |
|----------|------|-----|
| HTTPS ✅ | 7001 | https://localhost:7001 |
| HTTP  ⚠️ | 5001 | http://localhost:5001 |

For more details, see [SSL_SETUP.md](SSL_SETUP.md)
