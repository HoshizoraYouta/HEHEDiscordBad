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
[Route("api/[controller]")]
[Authorize]
public class ServersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServersController(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    public async Task<ActionResult<List<ServerDto>>> GetUserServers()
    {
        var userId = GetUserId();

        var servers = await _context.ServerMembers
            .Where(sm => sm.UserId == userId)
            .Include(sm => sm.Server)
            .Select(sm => new ServerDto
            {
                Id = sm.Server.Id,
                Name = sm.Server.Name,
                Description = sm.Server.Description,
                IconUrl = sm.Server.IconUrl,
                OwnerId = sm.Server.OwnerId,
                CreatedAt = sm.Server.CreatedAt
            })
            .ToListAsync();

        return Ok(servers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerDto>> GetServer(Guid id)
    {
        var userId = GetUserId();

        // Verify user is a member
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == id && sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var server = await _context.Servers.FindAsync(id);
        if (server == null)
        {
            return NotFound();
        }

        return Ok(new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Description = server.Description,
            IconUrl = server.IconUrl,
            OwnerId = server.OwnerId,
            CreatedAt = server.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ServerDto>> CreateServer([FromBody] CreateServerRequest request)
    {
        var userId = GetUserId();

        var server = new Server
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Servers.Add(server);

        // Create default roles
        var ownerRole = new Role
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            Name = "Owner",
            RoleType = RoleType.Owner,
            CreatedAt = DateTime.UtcNow
        };

        var memberRole = new Role
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            Name = "Member",
            RoleType = RoleType.Member,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.AddRange(ownerRole, memberRole);

        // Add owner as member
        var serverMember = new ServerMember
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            UserId = userId,
            RoleId = ownerRole.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.ServerMembers.Add(serverMember);

        // Create default channels
        var generalChannel = new Channel
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            Name = "general",
            Type = ChannelType.Text,
            CreatedAt = DateTime.UtcNow
        };

        var voiceChannel = new Channel
        {
            Id = Guid.NewGuid(),
            ServerId = server.Id,
            Name = "Voice",
            Type = ChannelType.Voice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Channels.AddRange(generalChannel, voiceChannel);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServer), new { id = server.Id }, new ServerDto
        {
            Id = server.Id,
            Name = server.Name,
            Description = server.Description,
            IconUrl = server.IconUrl,
            OwnerId = server.OwnerId,
            CreatedAt = server.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServer(Guid id)
    {
        var userId = GetUserId();
        var server = await _context.Servers.FindAsync(id);

        if (server == null)
        {
            return NotFound();
        }

        if (server.OwnerId != userId)
        {
            return Forbid();
        }

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<ServerMemberDto>>> GetServerMembers(Guid id)
    {
        var userId = GetUserId();

        // Verify user is a member
        var isMember = await _context.ServerMembers
            .AnyAsync(sm => sm.ServerId == id && sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var members = await _context.ServerMembers
            .Where(sm => sm.ServerId == id)
            .Include(sm => sm.User.Profile)
            .Include(sm => sm.Role)
            .Select(sm => new ServerMemberDto
            {
                Id = sm.Id,
                ServerId = sm.ServerId,
                UserId = sm.UserId,
                Username = sm.User.Profile!.Username,
                Nickname = sm.Nickname,
                RoleName = sm.Role.Name,
                JoinedAt = sm.JoinedAt
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost("{id}/invite")]
    public async Task<IActionResult> InviteUser(Guid id, [FromBody] InviteUserRequest request)
    {
        var userId = GetUserId();

        // Verify requester is owner or admin
        var requesterMember = await _context.ServerMembers
            .Include(sm => sm.Role)
            .FirstOrDefaultAsync(sm => sm.ServerId == id && sm.UserId == userId);

        if (requesterMember == null || 
            (requesterMember.Role.RoleType != RoleType.Owner && requesterMember.Role.RoleType != RoleType.Admin))
        {
            return Forbid();
        }

        // Check if user is already a member
        if (await _context.ServerMembers.AnyAsync(sm => sm.ServerId == id && sm.UserId == request.UserId))
        {
            return BadRequest(new { message = "User is already a member" });
        }

        // Get default member role
        var memberRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.ServerId == id && r.RoleType == RoleType.Member);

        if (memberRole == null)
        {
            return BadRequest(new { message = "Server configuration error" });
        }

        // Add user to server
        var serverMember = new ServerMember
        {
            Id = Guid.NewGuid(),
            ServerId = id,
            UserId = request.UserId,
            RoleId = memberRole.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.ServerMembers.Add(serverMember);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User invited successfully" });
    }
}
