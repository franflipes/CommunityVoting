using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityVoting.Maui.Models;

namespace CommunityVoting.Maui.Services;

public class MeetingService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public MeetingService(AuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient { BaseAddress = new Uri(AuthService.GetBaseUrl()) };
    }

    private void EnsureAuthHeader()
    {
        var token = _authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<List<MeetingItem>> GetMeetingsAsync()
    {
        var communityIdStr = _authService.GetCommunityId();
        if (!string.IsNullOrEmpty(communityIdStr) && Guid.TryParse(communityIdStr, out var communityId))
        {
            return await GetMeetingsByCommunityAsync(communityId);
        }

        try
        {
            EnsureAuthHeader();
            var result = await _httpClient.GetFromJsonAsync<List<MeetingItem>>("/api/meetings");
            return result ?? new List<MeetingItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMeetings Exception: {ex.Message}");
            return new List<MeetingItem>();
        }
    }

    public async Task<List<MeetingItem>> GetMeetingsByCommunityAsync(Guid communityId)
    {
        try
        {
            EnsureAuthHeader();
            var result = await _httpClient.GetFromJsonAsync<List<MeetingItem>>($"/api/meetings/community/{communityId}");
            return result ?? new List<MeetingItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMeetingsByCommunity Exception: {ex.Message}");
            return new List<MeetingItem>();
        }
    }

    public async Task<MeetingItem?> GetMeetingDetailsAsync(Guid meetingId)
    {
        try
        {
            EnsureAuthHeader();
            return await _httpClient.GetFromJsonAsync<MeetingItem>($"/api/meetings/{meetingId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMeetingDetails Exception: {ex.Message}");
            return null;
        }
    }
}
