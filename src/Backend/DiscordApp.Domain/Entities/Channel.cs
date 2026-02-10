using DiscordApp.Domain.Enums;

namespace DiscordApp.Domain.Entities;

public class Channel
{
    public Guid Id { get; set; }
    public Guid ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public ChannelType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Server Server { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<VoiceChannelSession> VoiceSessions { get; set; } = new List<VoiceChannelSession>();
}
