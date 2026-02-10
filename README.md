# Discord-like Application

A full-stack Discord-like application built with ASP.NET Core Web API backend and Blazor WebAssembly frontend.

## Architecture

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Real-time Communication**: SignalR
- **Authentication**: JWT (Access + Refresh tokens)
- **Database**: PostgreSQL with Entity Framework Core
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)

## Features

### Core Features
- User registration & login with JWT authentication
- User profiles with username, avatar upload, bio, and status
- Server (guild) management: create, delete, and invite users
- Role-based access control (Owner, Admin, Member)
- Text and voice channels within servers
- Real-time text messaging via SignalR
- Direct messages between users
- Voice channel presence tracking

### Backend
- Clean Architecture with separated layers
- REST API endpoints for all CRUD operations
- SignalR hubs for real-time communication
- Authorization policies per server role
- Input validation & error handling
- PostgreSQL database with proper relationships and indexes

### Frontend
- Login & Register pages
- Server list sidebar
- Channel management
- Real-time chat interface
- Direct messaging
- Responsive UI with Discord-like design

## Prerequisites

- .NET 8 SDK
- PostgreSQL 14 or higher
- Visual Studio 2022, VS Code, or Rider (optional)

## Common Issues

- **SSL/HTTPS errors?** See [SSL_SETUP.md](SSL_SETUP.md)
- **CORS errors?** See [CORS_TROUBLESHOOTING.md](CORS_TROUBLESHOOTING.md)
- **Quick start?** See [QUICK_START_URLS.md](QUICK_START_URLS.md)
- **Voice chat?** See [VOICE_CHAT_GUIDE.md](VOICE_CHAT_GUIDE.md)

## Setup Instructions

### 1. Database Setup

1. Install PostgreSQL if not already installed
2. Create a database:
```bash
createdb discordapp
```

3. Update the connection string in `src/Backend/DiscordApp.API/appsettings.json` if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=discordapp;Username=postgres;Password=postgres"
}
```

4. Run migrations:
```bash
cd src/Backend/DiscordApp.Infrastructure
dotnet ef database update --startup-project ../DiscordApp.API/DiscordApp.API.csproj
```

### 2. Backend Setup

**Important:** If you encounter SSL/HTTPS errors, see [SSL_SETUP.md](SSL_SETUP.md) for troubleshooting.

1. Trust the development certificate (first time only):
```bash
dotnet dev-certs https --trust
```

2. Navigate to the API project:
```bash
cd src/Backend/DiscordApp.API
```

3. Run the backend:
```bash
dotnet run
```

The API will be available at `https://localhost:7001` and `http://localhost:5001`.

Swagger documentation is available at `https://localhost:7001/swagger`.

### 3. Frontend Setup

1. Navigate to the client project:
```bash
cd src/Frontend/DiscordApp.Client
```

2. Update the API URL in `wwwroot/appsettings.json` if needed:
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

3. Run the frontend:
```bash
dotnet run
```

The frontend will be available at `https://localhost:5173` or the URL shown in the console.

### 4. Running Both Together

You can run both projects simultaneously:

**Terminal 1 (Backend):**
```bash
cd src/Backend/DiscordApp.API
dotnet run
```

**Terminal 2 (Frontend):**
```bash
cd src/Frontend/DiscordApp.Client
dotnet run
```

## Project Structure

```
├── src/
│   ├── Backend/
│   │   ├── DiscordApp.Domain/        # Domain entities and enums
│   │   ├── DiscordApp.Application/   # DTOs and interfaces
│   │   ├── DiscordApp.Infrastructure/ # Data access, services, migrations
│   │   └── DiscordApp.API/           # Controllers, SignalR hubs, configuration
│   └── Frontend/
│       └── DiscordApp.Client/        # Blazor WebAssembly app
```

## Key Technologies

- **Backend**:
  - ASP.NET Core 8.0
  - Entity Framework Core 8.0
  - Npgsql (PostgreSQL driver)
  - SignalR
  - JWT Bearer Authentication
  - BCrypt.Net for password hashing
  - Swashbuckle for Swagger/OpenAPI

- **Frontend**:
  - Blazor WebAssembly 8.0
  - Blazored.LocalStorage
  - SignalR Client
  - Bootstrap (optional)

## API Endpoints

### Authentication
- `POST /api/Auth/register` - Register new user
- `POST /api/Auth/login` - Login
- `POST /api/Auth/refresh` - Refresh token
- `GET /api/Auth/me` - Get current user

### Servers
- `GET /api/Servers` - Get user's servers
- `GET /api/Servers/{id}` - Get server details
- `POST /api/Servers` - Create server
- `DELETE /api/Servers/{id}` - Delete server
- `GET /api/Servers/{id}/members` - Get server members
- `POST /api/Servers/{id}/invite` - Invite user to server

### Channels
- `GET /api/Servers/{serverId}/Channels` - Get server channels
- `POST /api/Servers/{serverId}/Channels` - Create channel
- `GET /api/Servers/{serverId}/Channels/{channelId}/messages` - Get channel messages

### Direct Messages
- `GET /api/DirectMessages/conversations` - Get all conversations
- `GET /api/DirectMessages/with/{userId}` - Get messages with specific user

### Users
- `GET /api/Users/{id}` - Get user profile
- `PUT /api/Users/profile` - Update profile
- `POST /api/Users/avatar` - Upload avatar

## SignalR Hubs

### Chat Hub (`/hubs/chat`)
- `JoinServer(serverId)` - Join server group
- `JoinChannel(channelId)` - Join channel group
- `SendMessage(channelId, content)` - Send message to channel
- `SendDirectMessage(recipientId, content)` - Send direct message
- `TypingInChannel(channelId)` - Send typing indicator

### Voice Hub (`/hubs/voice`)
- `JoinVoiceChannel(channelId)` - Join voice channel
- `LeaveVoiceChannel(channelId)` - Leave voice channel
- `SendVoiceSignal(channelId, targetUserId, signal)` - WebRTC signaling

## Database Schema

The application uses PostgreSQL with the following main tables:
- Users
- UserProfiles
- Servers
- ServerMembers
- Roles
- Channels
- Messages
- DirectMessages
- VoiceChannelSessions
- Friendships

All tables have proper primary keys, foreign keys, indexes, and cascade rules.

## Security Features

- JWT-based authentication
- Password hashing with BCrypt
- Refresh token rotation
- Role-based authorization
- CORS configuration
- SQL injection protection via EF Core

## Development Notes

- The application uses Clean Architecture principles
- All code is production-ready with no TODOs or placeholders
- File upload support for avatars (local filesystem with S3-compatible abstraction)
- Real-time messaging is fully implemented with SignalR
- Voice channels include presence tracking (actual WebRTC voice is client-side)

## Testing the Application

1. Register a new user
2. Create a server
3. Create channels (text and voice)
4. Send messages in real-time
5. Test direct messaging
6. Invite other users to your server

## Future Enhancements

While the current implementation is fully functional, potential enhancements include:
- WebRTC video/audio implementation for voice channels
- Message editing and deletion
- File attachments in messages
- Emoji reactions
- Server roles customization
- User presence indicators
- Message search functionality

## License

This project is for educational purposes.
