using System.Security.Claims;
using DiscordApp.Application.DTOs;
using DiscordApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscordApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DirectMessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DirectMessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<object>>> GetConversations()
    {
        var userId = GetUserId();

        // Get all unique users the current user has exchanged messages with
        var sentToUsers = await _context.DirectMessages
            .Where(dm => dm.SenderId == userId)
            .Select(dm => dm.RecipientId)
            .Distinct()
            .ToListAsync();

        var receivedFromUsers = await _context.DirectMessages
            .Where(dm => dm.RecipientId == userId)
            .Select(dm => dm.SenderId)
            .Distinct()
            .ToListAsync();

        var allUserIds = sentToUsers.Union(receivedFromUsers).Distinct().ToList();

        var conversations = new List<object>();
        foreach (var otherUserId in allUserIds)
        {
            var lastMessage = await _context.DirectMessages
                .Where(dm => (dm.SenderId == userId && dm.RecipientId == otherUserId) ||
                           (dm.SenderId == otherUserId && dm.RecipientId == userId))
                .OrderByDescending(dm => dm.CreatedAt)
                .FirstOrDefaultAsync();

            var otherUser = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == otherUserId);

            if (lastMessage != null && otherUser?.Profile != null)
            {
                conversations.Add(new
                {
                    userId = otherUserId,
                    username = otherUser.Profile.Username,
                    avatarUrl = otherUser.Profile.AvatarUrl,
                    lastMessage = lastMessage.Content,
                    lastMessageTime = lastMessage.CreatedAt,
                    unreadCount = await _context.DirectMessages
                        .CountAsync(dm => dm.SenderId == otherUserId && 
                                        dm.RecipientId == userId && 
                                        !dm.IsRead)
                });
            }
        }

        return Ok(conversations.OrderByDescending(c => ((dynamic)c).lastMessageTime));
    }

    [HttpGet("with/{userId}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetMessagesWithUser(Guid userId, [FromQuery] int limit = 50)
    {
        var currentUserId = GetUserId();

        var messages = await _context.DirectMessages
            .Where(dm => (dm.SenderId == currentUserId && dm.RecipientId == userId) ||
                        (dm.SenderId == userId && dm.RecipientId == currentUserId))
            .Where(dm => !dm.IsDeleted)
            .OrderByDescending(dm => dm.CreatedAt)
            .Take(limit)
            .Include(dm => dm.Sender.Profile)
            .Select(dm => new DirectMessageDto
            {
                Id = dm.Id,
                SenderId = dm.SenderId,
                RecipientId = dm.RecipientId,
                SenderUsername = dm.Sender.Profile!.Username,
                SenderAvatar = dm.Sender.Profile.AvatarUrl,
                Content = dm.Content,
                CreatedAt = dm.CreatedAt,
                IsRead = dm.IsRead
            })
            .ToListAsync();

        // Mark messages as read
        var unreadMessages = await _context.DirectMessages
            .Where(dm => dm.SenderId == userId && 
                        dm.RecipientId == currentUserId && 
                        !dm.IsRead)
            .ToListAsync();

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }

        if (unreadMessages.Any())
        {
            await _context.SaveChangesAsync();
        }

        messages.Reverse(); // Return in chronological order
        return Ok(messages);
    }
}
