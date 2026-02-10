using DiscordApp.Client.Models;
using System.Net.Http.Json;

namespace DiscordApp.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Servers
    public async Task<List<ServerDto>?> GetServersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ServerDto>>("api/Servers");
    }

    public async Task<ServerDto?> GetServerAsync(Guid serverId)
    {
        return await _httpClient.GetFromJsonAsync<ServerDto>($"api/Servers/{serverId}");
    }

    public async Task<ServerDto?> CreateServerAsync(CreateServerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Servers", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ServerDto>();
        }
        return null;
    }

    public async Task<bool> DeleteServerAsync(Guid serverId)
    {
        var response = await _httpClient.DeleteAsync($"api/Servers/{serverId}");
        return response.IsSuccessStatusCode;
    }

    // Channels
    public async Task<List<ChannelDto>?> GetChannelsAsync(Guid serverId)
    {
        return await _httpClient.GetFromJsonAsync<List<ChannelDto>>($"api/Servers/{serverId}/Channels");
    }

    public async Task<ChannelDto?> CreateChannelAsync(Guid serverId, CreateChannelRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Servers/{serverId}/Channels", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ChannelDto>();
        }
        return null;
    }

    // Messages
    public async Task<List<MessageDto>?> GetChannelMessagesAsync(Guid serverId, Guid channelId, int limit = 50)
    {
        return await _httpClient.GetFromJsonAsync<List<MessageDto>>(
            $"api/Servers/{serverId}/Channels/{channelId}/messages?limit={limit}");
    }

    // Direct Messages
    public async Task<List<object>?> GetConversationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<object>>("api/DirectMessages/conversations");
    }

    public async Task<List<DirectMessageDto>?> GetDirectMessagesAsync(Guid userId, int limit = 50)
    {
        return await _httpClient.GetFromJsonAsync<List<DirectMessageDto>>(
            $"api/DirectMessages/with/{userId}?limit={limit}");
    }

    // User Profile
    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{userId}");
    }
}
