using System.Security.Claims;
using DiscordApp.Application.DTOs;
using DiscordApp.Application.Interfaces;
using DiscordApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiscordApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UsersController(ApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Profile = user.Profile != null ? new UserProfileDto
            {
                Id = user.Profile.Id,
                Username = user.Profile.Username,
                AvatarUrl = user.Profile.AvatarUrl,
                Bio = user.Profile.Bio,
                Status = user.Profile.Status
            } : null
        });
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            profile.Username = request.Username;
        }
        if (request.Bio != null)
        {
            profile.Bio = request.Bio;
        }
        if (request.Status != null)
        {
            profile.Status = request.Status;
        }

        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new UserProfileDto
        {
            Id = profile.Id,
            Username = profile.Username,
            AvatarUrl = profile.AvatarUrl,
            Bio = profile.Bio,
            Status = profile.Status
        });
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var userId = GetUserId();
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            await _fileStorage.DeleteFileAsync(profile.AvatarUrl);
        }

        // Upload new avatar
        using var stream = file.OpenReadStream();
        var url = await _fileStorage.UploadFileAsync(stream, file.FileName, file.ContentType);

        profile.AvatarUrl = url;
        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { avatarUrl = url });
    }
}
