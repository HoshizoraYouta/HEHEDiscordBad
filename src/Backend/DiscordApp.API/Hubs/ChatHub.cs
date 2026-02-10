using System.Security.Claims;
using DiscordApp.Application.DTOs;
using DiscordApp.Domain.Entities;
using DiscordApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DiscordApp.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }

    public async Task JoinServer(Guid serverId)
    {
        var userId = GetUserId();
        
        // Verify user is a member of the server
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (isMember)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"server-{serverId}");
        }
    }

    public async Task LeaveServer(Guid serverId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"server-{serverId}");
    }

    public async Task JoinChannel(Guid channelId)
    {
        var userId = GetUserId();
        
        // Verify user has access to the channel
        var channel = await _context.Channels
            .Include(c => c.Server)
            .ThenInclude(s => s.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel != null && channel.Server.Members.Any(m => m.UserId == userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"channel-{channelId}");
        }
    }

    public async Task LeaveChannel(Guid channelId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"channel-{channelId}");
    }

    public async Task SendMessage(Guid channelId, string content)
    {
        var userId = GetUserId();

        // Verify user has access to the channel
        var channel = await _context.Channels
            .Include(c => c.Server)
            .ThenInclude(s => s.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null || !channel.Server.Members.Any(m => m.UserId == userId))
        {
            throw new UnauthorizedAccessException("You don't have access to this channel");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Load user profile for the response
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            ChannelId = message.ChannelId,
            UserId = message.UserId,
            Username = userProfile?.Username ?? "Unknown",
            UserAvatar = userProfile?.AvatarUrl,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            EditedAt = message.EditedAt
        };

        await Clients.Group($"channel-{channelId}").SendAsync("ReceiveMessage", messageDto);
    }

    public async Task SendDirectMessage(Guid recipientId, string content)
    {
        var senderId = GetUserId();

        var directMessage = new DirectMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsRead = false
        };

        _context.DirectMessages.Add(directMessage);
        await _context.SaveChangesAsync();

        // Load sender profile
        var senderProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == senderId);

        var messageDto = new DirectMessageDto
        {
            Id = directMessage.Id,
            SenderId = senderId,
            RecipientId = recipientId,
            SenderUsername = senderProfile?.Username ?? "Unknown",
            SenderAvatar = senderProfile?.AvatarUrl,
            Content = content,
            CreatedAt = directMessage.CreatedAt,
            IsRead = false
        };

        // Send to both sender and recipient
        await Clients.Group($"user-{senderId}").SendAsync("ReceiveDirectMessage", messageDto);
        await Clients.Group($"user-{recipientId}").SendAsync("ReceiveDirectMessage", messageDto);
    }

    public async Task TypingInChannel(Guid channelId)
    {
        var userId = GetUserId();
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        
        await Clients.OthersInGroup($"channel-{channelId}")
            .SendAsync("UserTyping", channelId, userId, userProfile?.Username ?? "Unknown");
    }
}
