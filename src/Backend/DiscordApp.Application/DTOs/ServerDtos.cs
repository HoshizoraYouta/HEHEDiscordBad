namespace DiscordApp.Application.DTOs;

public class ServerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateServerRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class InviteUserRequest
{
    public Guid UserId { get; set; }
}

public class ServerMemberDto
{
    public Guid Id { get; set; }
    public Guid ServerId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
