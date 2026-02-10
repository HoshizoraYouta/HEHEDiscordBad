namespace DiscordApp.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation properties
    public UserProfile? Profile { get; set; }
    public ICollection<ServerMember> ServerMemberships { get; set; } = new List<ServerMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<DirectMessage> SentDirectMessages { get; set; } = new List<DirectMessage>();
    public ICollection<DirectMessage> ReceivedDirectMessages { get; set; } = new List<DirectMessage>();
    public ICollection<Friendship> InitiatedFriendships { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendships { get; set; } = new List<Friendship>();
}
