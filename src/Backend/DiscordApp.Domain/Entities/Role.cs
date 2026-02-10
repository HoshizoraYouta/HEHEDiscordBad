using DiscordApp.Domain.Enums;

namespace DiscordApp.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public Guid ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Server Server { get; set; } = null!;
    public ICollection<ServerMember> Members { get; set; } = new List<ServerMember>();
}
