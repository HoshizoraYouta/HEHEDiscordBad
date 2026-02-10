using System.Security.Claims;
using DiscordApp.Application.DTOs;
using DiscordApp.Domain.Entities;
using DiscordApp.Domain.Enums;
using DiscordApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscordApp.API.Controllers;

[ApiController]
[Route("api/servers/{serverId}/[controller]")]
[Authorize]
public class ChannelsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChannelsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    public async Task<ActionResult<List<ChannelDto>>> GetServerChannels(Guid serverId)
    {
        var userId = GetUserId();

        // Verify user is a member of the server
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var channels = await _context.Channels
            .Where(c => c.ServerId == serverId)
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                ServerId = c.ServerId,
                Name = c.Name,
                Topic = c.Topic,
                Type = c.Type.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(channels);
    }

    [HttpGet("{channelId}")]
    public async Task<ActionResult<ChannelDto>> GetChannel(Guid serverId, Guid channelId)
    {
        var userId = GetUserId();

        // Verify user is a member of the server
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null || channel.ServerId != serverId)
        {
            return NotFound();
        }

        return Ok(new ChannelDto
        {
            Id = channel.Id,
            ServerId = channel.ServerId,
            Name = channel.Name,
            Topic = channel.Topic,
            Type = channel.Type.ToString(),
            CreatedAt = channel.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ChannelDto>> CreateChannel(Guid serverId, [FromBody] CreateChannelRequest request)
    {
        var userId = GetUserId();

        // Verify user is owner or admin
        var member = await _context.ServerMembers
            .Include(sm => sm.Role)
            .FirstOrDefaultAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (member == null || 
            (member.Role.RoleType != RoleType.Owner && member.Role.RoleType != RoleType.Admin))
        {
            return Forbid();
        }

        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            ServerId = serverId,
            Name = request.Name,
            Topic = request.Topic,
            Type = Enum.Parse<ChannelType>(request.Type),
            CreatedAt = DateTime.UtcNow
        };

        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChannel), new { serverId, channelId = channel.Id }, new ChannelDto
        {
            Id = channel.Id,
            ServerId = channel.ServerId,
            Name = channel.Name,
            Topic = channel.Topic,
            Type = channel.Type.ToString(),
            CreatedAt = channel.CreatedAt
        });
    }

    [HttpDelete("{channelId}")]
    public async Task<IActionResult> DeleteChannel(Guid serverId, Guid channelId)
    {
        var userId = GetUserId();

        // Verify user is owner or admin
        var member = await _context.ServerMembers
            .Include(sm => sm.Role)
            .FirstOrDefaultAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (member == null || 
            (member.Role.RoleType != RoleType.Owner && member.Role.RoleType != RoleType.Admin))
        {
            return Forbid();
        }

        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null || channel.ServerId != serverId)
        {
            return NotFound();
        }

        _context.Channels.Remove(channel);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{channelId}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetChannelMessages(Guid serverId, Guid channelId, [FromQuery] int limit = 50)
    {
        var userId = GetUserId();

        // Verify user is a member of the server
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == serverId && sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var messages = await _context.Messages
            .Where(m => m.ChannelId == channelId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Include(m => m.User.Profile)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ChannelId = m.ChannelId,
                UserId = m.UserId,
                Username = m.User.Profile!.Username,
                UserAvatar = m.User.Profile.AvatarUrl,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                EditedAt = m.EditedAt
            })
            .ToListAsync();

        messages.Reverse(); // Return in chronological order
        return Ok(messages);
    }
}
