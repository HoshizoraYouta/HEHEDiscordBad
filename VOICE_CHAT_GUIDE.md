# Voice Chat Guide

## Overview

The voice chat feature allows users to join voice channels within servers and see who else is connected. This implementation provides **presence tracking and signaling infrastructure** - the foundation for voice communication.

## What's Included

### Backend (Complete)
- ✅ **VoiceHub**: SignalR hub for voice channel management
- ✅ **Join/Leave Voice Channel**: Track user presence in voice channels
- ✅ **VoiceChannelSession**: Database entity tracking active sessions
- ✅ **Real-time Notifications**: Notify when users join/leave voice channels
- ✅ **Automatic Cleanup**: Sessions end when user disconnects

### Frontend (Now Working)
- ✅ **Voice Hub Connection**: SignalR connection to voice hub
- ✅ **Join/Leave UI**: Click voice channels to join/leave
- ✅ **Visual Indicators**: 
  - Green dot (●) shows when you're in a voice channel
  - Active channel is highlighted
- ✅ **User List**: See who else is in each voice channel
- ✅ **Real-time Updates**: Instantly see when users join/leave

## How to Use

### 1. Join a Voice Channel

1. Navigate to a server with voice channels
2. Look for voice channels with the 🔊 icon in the sidebar
3. **Click on a voice channel** to join it
4. You'll see a **green dot (●)** appear next to the channel name
5. The channel will be **highlighted** to show you're connected

### 2. See Who's In a Voice Channel

- Users currently in a voice channel are listed under the channel name
- Each user is shown with a 🎤 microphone icon

### 3. Leave a Voice Channel

- **Click the same voice channel again** to leave
- The green dot will disappear
- You'll be removed from the user list

### 4. Switch Voice Channels

- Simply click a different voice channel
- You'll automatically leave the current one and join the new one

## Technical Details

### How It Works

1. **Joining**:
   - Frontend calls `JoinVoiceChannel(channelId)` via SignalR
   - Backend creates a `VoiceChannelSession` in the database
   - Backend adds user to SignalR group `voice-{channelId}`
   - All users in the channel receive `UserJoinedVoice` event

2. **Leaving**:
   - Frontend calls `LeaveVoiceChannel(channelId)` via SignalR
   - Backend marks session as inactive
   - Backend removes user from SignalR group
   - All users in the channel receive `UserLeftVoice` event

3. **Automatic Cleanup**:
   - If connection drops, backend automatically cleans up sessions
   - `OnDisconnectedAsync` marks all user's sessions as inactive

### SignalR Events

**Client -> Server:**
- `JoinVoiceChannel(channelId)` - Join a voice channel
- `LeaveVoiceChannel(channelId)` - Leave a voice channel
- `SendVoiceSignal(channelId, targetUserId, signal)` - WebRTC signaling (for future audio)

**Server -> Client:**
- `UserJoinedVoice(VoiceSessionDto)` - Someone joined the channel
- `UserLeftVoice(channelId, userId)` - Someone left the channel
- `ReceiveVoiceSignal(userId, targetUserId, signal)` - WebRTC signal received (for future audio)

## What's NOT Included (Yet)

This implementation provides the **infrastructure** for voice chat, but does **not** include:

❌ **Actual Audio Transmission**: WebRTC audio/video streaming
❌ **Microphone Access**: Browser microphone permissions and capture
❌ **Audio Playback**: Playing other users' audio streams
❌ **Voice Settings**: Mute, deafen, volume controls
❌ **Screen Sharing**: Video streaming capabilities

These features require additional WebRTC implementation and are beyond the scope of this presence tracking system.

## Adding Full Voice Chat (Future Enhancement)

To implement actual voice communication, you would need to:

1. **Add WebRTC Library**: Use a library like `simple-peer` or native WebRTC APIs
2. **Implement Peer Connections**: Create WebRTC peer connections between users
3. **Use Signaling**: Use the existing `SendVoiceSignal` method for WebRTC signaling (offers, answers, ICE candidates)
4. **Capture Audio**: Access user's microphone via `getUserMedia()`
5. **Stream Audio**: Send audio streams to peers and play received streams
6. **Add UI Controls**: Mute, deafen, volume, etc.

### Example WebRTC Flow (for future implementation)

```javascript
// 1. Get user's microphone
const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

// 2. Create peer connection
const peerConnection = new RTCPeerConnection();
stream.getTracks().forEach(track => peerConnection.addTrack(track, stream));

// 3. Create and send offer
const offer = await peerConnection.createOffer();
await peerConnection.setLocalDescription(offer);
await signalRService.SendVoiceSignal(channelId, targetUserId, JSON.stringify(offer));

// 4. Handle incoming signals via "ReceiveVoiceSignal" event
// 5. Play remote audio stream when received
```

## Current Capabilities

What you **CAN** do now:
- ✅ Join and leave voice channels
- ✅ See who's in each voice channel in real-time
- ✅ Track voice channel presence
- ✅ Get notifications when users join/leave
- ✅ Have the infrastructure for future WebRTC implementation

What you **CANNOT** do (yet):
- ❌ Hear other users
- ❌ Talk to other users
- ❌ Share your microphone
- ❌ Adjust audio settings

## Database

Voice channel sessions are stored in the `VoiceChannelSessions` table:

```sql
- Id (Guid)
- ChannelId (Guid) - Which channel
- UserId (Guid) - Who joined
- JoinedAt (DateTime) - When they joined
- LeftAt (DateTime?) - When they left (null if still active)
- IsActive (bool) - Currently in channel
```

## API Endpoints

While voice channels use SignalR for real-time communication, you can also:

- Create voice channels via REST API (same as text channels, but with `Type: "Voice"`)
- View voice channel sessions via database queries

## Troubleshooting

### "Voice channels don't show up"
- Make sure you've created channels with `Type: "Voice"`
- Check that you're a member of the server

### "Can't join voice channel"
- Ensure both backend and frontend are running
- Check browser console for SignalR connection errors
- Verify you're logged in and have access to the server

### "Don't see other users in voice"
- Make sure SignalR voice hub is connected
- Check that the backend VoiceHub is mapped in Program.cs: `app.MapHub<VoiceHub>("/hubs/voice")`
- Verify the CORS policy allows the frontend origin

### "Nothing happens when I click voice channel"
- Check browser console for JavaScript errors
- Verify SignalR connection is established
- Look for authentication token issues

## Testing Voice Chat

1. **Start the backend**:
   ```bash
   cd src/Backend/DiscordApp.API
   dotnet run
   ```

2. **Start the frontend**:
   ```bash
   cd src/Frontend/DiscordApp.Client
   dotnet run
   ```

3. **Test with multiple browser tabs**:
   - Open the app in multiple tabs (or browsers)
   - Login with different users
   - Join the same server
   - Click a voice channel in one tab
   - See the user appear in the other tab's voice channel user list

4. **Verify functionality**:
   - ✅ Green dot appears when you join
   - ✅ Your username shows under the channel
   - ✅ Other users see you join in real-time
   - ✅ When you leave, the dot disappears
   - ✅ Other users see you leave in real-time

## Summary

This implementation provides a **working voice channel presence system** that:
- Tracks who's in each voice channel
- Shows real-time join/leave notifications
- Provides infrastructure for future WebRTC audio implementation
- Works across multiple users and browser tabs

The voice chat **presence and signaling are fully functional**. Adding actual audio transmission would require implementing WebRTC peer connections and media streaming, which is a separate, more complex feature.
