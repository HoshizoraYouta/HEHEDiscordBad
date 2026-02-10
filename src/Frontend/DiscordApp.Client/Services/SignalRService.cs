using Blazored.LocalStorage;
using DiscordApp.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace DiscordApp.Client.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _chatHubConnection;
    private HubConnection? _voiceHubConnection;
    private readonly ILocalStorageService _localStorage;
    private readonly string _baseUrl;

    public event Action<MessageDto>? OnMessageReceived;
    public event Action<DirectMessageDto>? OnDirectMessageReceived;
    public event Action<Guid, Guid, string>? OnUserTyping;

    public SignalRService(ILocalStorageService localStorage, IConfiguration configuration)
    {
        _localStorage = localStorage;
        _baseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7001";
    }

    public async Task InitializeAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        // Initialize Chat Hub
        _chatHubConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseUrl}/hubs/chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _chatHubConnection.On<MessageDto>("ReceiveMessage", message =>
        {
            OnMessageReceived?.Invoke(message);
        });

        _chatHubConnection.On<DirectMessageDto>("ReceiveDirectMessage", message =>
        {
            OnDirectMessageReceived?.Invoke(message);
        });

        _chatHubConnection.On<Guid, Guid, string>("UserTyping", (channelId, userId, username) =>
        {
            OnUserTyping?.Invoke(channelId, userId, username);
        });

        try
        {
            await _chatHubConnection.StartAsync();
        }
        catch
        {
            // Connection failed
        }
    }

    public async Task JoinServerAsync(Guid serverId)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("JoinServer", serverId);
        }
    }

    public async Task JoinChannelAsync(Guid channelId)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("JoinChannel", channelId);
        }
    }

    public async Task LeaveChannelAsync(Guid channelId)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("LeaveChannel", channelId);
        }
    }

    public async Task SendMessageAsync(Guid channelId, string content)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("SendMessage", channelId, content);
        }
    }

    public async Task SendDirectMessageAsync(Guid recipientId, string content)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("SendDirectMessage", recipientId, content);
        }
    }

    public async Task SendTypingIndicatorAsync(Guid channelId)
    {
        if (_chatHubConnection?.State == HubConnectionState.Connected)
        {
            await _chatHubConnection.InvokeAsync("TypingInChannel", channelId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_chatHubConnection != null)
        {
            await _chatHubConnection.DisposeAsync();
        }
        if (_voiceHubConnection != null)
        {
            await _voiceHubConnection.DisposeAsync();
        }
    }
}
