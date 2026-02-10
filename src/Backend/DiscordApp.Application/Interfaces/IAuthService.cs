using DiscordApp.Application.DTOs;

namespace DiscordApp.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
}
