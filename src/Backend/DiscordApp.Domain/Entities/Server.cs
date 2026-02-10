namespace DiscordApp.Domain.Entities;

public class Server
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ServerMember> Members { get; set; } = new List<ServerMember>();
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
