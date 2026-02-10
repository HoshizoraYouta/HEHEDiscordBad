namespace DiscordApp.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Status { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
