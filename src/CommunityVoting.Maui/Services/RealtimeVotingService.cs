using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityVoting.Maui.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace CommunityVoting.Maui.Services;

public class RealtimeVotingService
{
    private HubConnection? _hubConnection;
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;

    public event Action<VotingSessionModel>? SessionOpened;
    public event Action<VoteCastNotification>? VoteCastReceived;
    public event Action<string>? SessionClosed;

    public RealtimeVotingService(AuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient { BaseAddress = new Uri(GetVotingApiBaseUrl()) };
    }

    public static string GetVotingApiBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5222";
        return "http://localhost:5222";
    }

    public async Task InitializeAsync()
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            return;

        var hubUrl = $"{GetVotingApiBaseUrl()}/hubs/voting";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_authService.GetToken());
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<VotingSessionModel>("OnSessionOpened", session =>
        {
            SessionOpened?.Invoke(session);
        });

        _hubConnection.On<VoteCastNotification>("OnVoteCast", notification =>
        {
            VoteCastReceived?.Invoke(notification);
        });

        _hubConnection.On<string>("OnSessionClosed", sessionId =>
        {
            SessionClosed?.Invoke(sessionId);
        });

        try
        {
            await _hubConnection.StartAsync();
            System.Diagnostics.Debug.WriteLine("[SignalR] Connected to Voting Hub.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SignalR] Connection Error: {ex.Message}");
        }
    }

    public async Task JoinMeetingGroupAsync(string meetingId)
    {
        if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            await InitializeAsync();

        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinMeetingGroup", meetingId);
            System.Diagnostics.Debug.WriteLine($"[SignalR] Joined meeting group: meeting-{meetingId}");
        }
    }

    public async Task<bool> SubmitVoteAsync(string sessionId, string optionId)
    {
        try
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsJsonAsync($"/api/voting/sessions/{sessionId}/vote", new CastVoteRequest(sessionId, optionId));
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubmitVote Exception: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}
