namespace DiscordApp.Domain.Entities;

public class ServerMember
{
    public Guid Id { get; set; }
    public Guid ServerId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string? Nickname { get; set; }
    public DateTime JoinedAt { get; set; }
    
    // Navigation properties
    public Server Server { get; set; } = null!;
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
