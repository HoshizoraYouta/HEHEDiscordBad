using System.Security.Claims;
using DiscordApp.Application.DTOs;
using DiscordApp.Domain.Entities;
using DiscordApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DiscordApp.API.Hubs;

[Authorize]
public class VoiceHub : Hub
{
    private readonly ApplicationDbContext _context;

    public VoiceHub(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    public async Task JoinVoiceChannel(Guid channelId)
    {
        var userId = GetUserId();

        // Verify channel is a voice channel and user has access
        var channel = await _context.Channels
            .Include(c => c.Server)
            .ThenInclude(s => s.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null || channel.Type != Domain.Enums.ChannelType.Voice)
        {
            throw new InvalidOperationException("Invalid voice channel");
        }

        if (!channel.Server.Members.Any(m => m.UserId == userId))
        {
            throw new UnauthorizedAccessException("You don't have access to this channel");
        }

        // Create voice session
        var session = new VoiceChannelSession
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.VoiceChannelSessions.Add(session);
        await _context.SaveChangesAsync();

        // Join SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"voice-{channelId}");

        // Load user profile
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        var sessionDto = new VoiceSessionDto
        {
            Id = session.Id,
            ChannelId = channelId,
            UserId = userId,
            Username = userProfile?.Username ?? "Unknown",
            JoinedAt = session.JoinedAt
        };

        // Notify others in the channel
        await Clients.Group($"voice-{channelId}").SendAsync("UserJoinedVoice", sessionDto);
    }

    public async Task LeaveVoiceChannel(Guid channelId)
    {
        var userId = GetUserId();

        // End voice session
        var session = await _context.VoiceChannelSessions
            .FirstOrDefaultAsync(s => s.ChannelId == channelId && s.UserId == userId && s.IsActive);

        if (session != null)
        {
            session.IsActive = false;
            session.LeftAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Leave SignalR group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"voice-{channelId}");

        // Notify others in the channel
        await Clients.Group($"voice-{channelId}").SendAsync("UserLeftVoice", channelId, userId);
    }

    public async Task SendVoiceSignal(Guid channelId, Guid targetUserId, string signal)
    {
        // This is for WebRTC signaling (offer, answer, ICE candidates)
        var userId = GetUserId();
        
        await Clients.Group($"voice-{channelId}").SendAsync("ReceiveVoiceSignal", userId, targetUserId, signal);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        // End all active sessions for this user
        var activeSessions = await _context.VoiceChannelSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in activeSessions)
        {
            session.IsActive = false;
            session.LeftAt = DateTime.UtcNow;

            // Notify others
            await Clients.Group($"voice-{session.ChannelId}").SendAsync("UserLeftVoice", session.ChannelId, userId);
        }

        await _context.SaveChangesAsync();
        await base.OnDisconnectedAsync(exception);
    }
}
