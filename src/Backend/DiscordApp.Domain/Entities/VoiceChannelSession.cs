namespace DiscordApp.Domain.Entities;

public class VoiceChannelSession
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Channel Channel { get; set; } = null!;
    public User User { get; set; } = null!;
}
