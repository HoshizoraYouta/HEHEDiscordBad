using Blazored.LocalStorage;
using DiscordApp.Client.Models;
using System.Net.Http.Json;

namespace DiscordApp.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private UserDto? _currentUser;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public UserDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    public async Task<bool> InitializeAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            // Verify token is still valid by getting current user
            var user = await _localStorage.GetItemAsync<UserDto>("currentUser");
            if (user != null)
            {
                _currentUser = user;
                NotifyAuthStateChanged();
                return true;
            }
        }
        catch
        {
            // Token invalid or expired
            await LogoutAsync();
        }

        return false;
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await SetAuthDataAsync(loginResponse);
                    return loginResponse;
                }
            }
        }
        catch { }
        
        return null;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await SetAuthDataAsync(loginResponse);
                    return loginResponse;
                }
            }
        }
        catch { }
        
        return null;
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("currentUser");
        NotifyAuthStateChanged();
    }

    private async Task SetAuthDataAsync(LoginResponse response)
    {
        _currentUser = response.User;
        await _localStorage.SetItemAsync("accessToken", response.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
        await _localStorage.SetItemAsync("currentUser", response.User);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.AccessToken);
        NotifyAuthStateChanged();
    }

    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}
