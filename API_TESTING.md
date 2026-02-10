# API Testing Guide

This document provides example requests for testing the Discord-like Application API using curl, Postman, or any HTTP client.

## Base URL

- Local: `https://localhost:7001` or `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

## Authentication

All protected endpoints require a Bearer token in the Authorization header:
```
Authorization: Bearer <your_access_token>
```

## Endpoints

### 1. Register a New User

```bash
curl -X POST https://localhost:7001/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "username": "CoolUser"
  }'
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "profile": {
      "id": "guid",
      "username": "CoolUser",
      "avatarUrl": null,
      "bio": null,
      "status": "Online"
    }
  }
}
```

### 2. Login

```bash
curl -X POST https://localhost:7001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

### 3. Get Current User

```bash
curl -X GET https://localhost:7001/api/Auth/me \
  -H "Authorization: Bearer <token>"
```

### 4. Create a Server

```bash
curl -X POST https://localhost:7001/api/Servers \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Awesome Server",
    "description": "A place for awesome people"
  }'
```

**Response:**
```json
{
  "id": "guid",
  "name": "My Awesome Server",
  "description": "A place for awesome people",
  "iconUrl": null,
  "ownerId": "guid",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### 5. Get User's Servers

```bash
curl -X GET https://localhost:7001/api/Servers \
  -H "Authorization: Bearer <token>"
```

### 6. Create a Text Channel

```bash
curl -X POST https://localhost:7001/api/Servers/{serverId}/Channels \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "general-chat",
    "topic": "General discussion",
    "type": "Text"
  }'
```

### 7. Create a Voice Channel

```bash
curl -X POST https://localhost:7001/api/Servers/{serverId}/Channels \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Voice Room 1",
    "type": "Voice"
  }'
```

### 8. Get Server Channels

```bash
curl -X GET https://localhost:7001/api/Servers/{serverId}/Channels \
  -H "Authorization: Bearer <token>"
```

### 9. Get Channel Messages

```bash
curl -X GET "https://localhost:7001/api/Servers/{serverId}/Channels/{channelId}/messages?limit=50" \
  -H "Authorization: Bearer <token>"
```

### 10. Get Direct Message Conversations

```bash
curl -X GET https://localhost:7001/api/DirectMessages/conversations \
  -H "Authorization: Bearer <token>"
```

### 11. Get Direct Messages with a User

```bash
curl -X GET "https://localhost:7001/api/DirectMessages/with/{userId}?limit=50" \
  -H "Authorization: Bearer <token>"
```

### 12. Update User Profile

```bash
curl -X PUT https://localhost:7001/api/Users/profile \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "NewUsername",
    "bio": "I love coding!",
    "status": "Busy coding"
  }'
```

### 13. Upload Avatar

```bash
curl -X POST https://localhost:7001/api/Users/avatar \
  -H "Authorization: Bearer <token>" \
  -F "file=@/path/to/avatar.jpg"
```

### 14. Invite User to Server

```bash
curl -X POST https://localhost:7001/api/Servers/{serverId}/invite \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "guid-of-user-to-invite"
  }'
```

### 15. Get Server Members

```bash
curl -X GET https://localhost:7001/api/Servers/{serverId}/members \
  -H "Authorization: Bearer <token>"
```

### 16. Delete Server

```bash
curl -X DELETE https://localhost:7001/api/Servers/{serverId} \
  -H "Authorization: Bearer <token>"
```

### 17. Delete Channel

```bash
curl -X DELETE https://localhost:7001/api/Servers/{serverId}/Channels/{channelId} \
  -H "Authorization: Bearer <token>"
```

## SignalR Hubs

### Chat Hub

**URL:** `wss://localhost:7001/hubs/chat?access_token=<your_token>`

**Methods:**
- `JoinServer(serverId)` - Join a server group
- `JoinChannel(channelId)` - Join a channel group
- `LeaveChannel(channelId)` - Leave a channel group
- `SendMessage(channelId, content)` - Send a message to a channel
- `SendDirectMessage(recipientId, content)` - Send a direct message
- `TypingInChannel(channelId)` - Send typing indicator

**Events:**
- `ReceiveMessage(messageDto)` - Receive a channel message
- `ReceiveDirectMessage(directMessageDto)` - Receive a direct message
- `UserTyping(channelId, userId, username)` - User is typing

### Voice Hub

**URL:** `wss://localhost:7001/hubs/voice?access_token=<your_token>`

**Methods:**
- `JoinVoiceChannel(channelId)` - Join a voice channel
- `LeaveVoiceChannel(channelId)` - Leave a voice channel
- `SendVoiceSignal(channelId, targetUserId, signal)` - Send WebRTC signaling data

**Events:**
- `UserJoinedVoice(sessionDto)` - User joined voice channel
- `UserLeftVoice(channelId, userId)` - User left voice channel
- `ReceiveVoiceSignal(userId, targetUserId, signal)` - Receive WebRTC signaling data

## Testing with Postman

1. Import the Swagger definition from `https://localhost:7001/swagger/v1/swagger.json`
2. Create an environment with a variable `baseUrl` = `https://localhost:7001`
3. Register a user and save the `accessToken` to an environment variable
4. Add the token to the Authorization header for protected requests

## Testing Flow

1. **Register** two users (User A and User B)
2. **Login** as User A, save the token
3. **Create a server** as User A
4. **Create channels** (text and voice) in the server
5. **Join the server** with User A (automatic on creation)
6. **Login** as User B, save the token
7. **Invite User B** to the server (as User A)
8. **Connect to SignalR** as both users
9. **Join channel** via SignalR
10. **Send messages** and see real-time updates
11. **Send direct messages** between User A and User B

## Error Codes

- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no response body
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Notes

- All timestamps are in UTC
- GUIDs are used for all IDs
- Passwords must be at least 6 characters (adjust validation as needed)
- File uploads accept images (jpg, png, gif) up to 5MB
- Message content is limited to 2000 characters
