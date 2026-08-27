using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityVoting.Maui.Models;

namespace CommunityVoting.Maui.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user_name";
    private const string CommunityKey = "auth_community_id";

    public AuthService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) };
    }

    public static string GetBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5004";
        return "http://localhost:5004";
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                Preferences.Set(TokenKey, result.Token);
                var fullName = result.User != null ? $"{result.User.Name} {result.User.LastName}".Trim() : email;
                Preferences.Set(UserKey, fullName);

                // Fetch and save user's primary community ID
                await FetchAndSaveCommunityIdAsync(result.Token);

                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Exception: {ex.Message}");
            return false;
        }
    }

    private async Task FetchAndSaveCommunityIdAsync(string token)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var communities = await client.GetFromJsonAsync<List<CommunityItem>>("/api/communities");
            if (communities != null && communities.Any())
            {
                Preferences.Set(CommunityKey, communities.First().Id.ToString());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FetchCommunities Exception: {ex.Message}");
        }
    }

    public string? GetToken() => Preferences.Get(TokenKey, null);
    public string? GetUserName() => Preferences.Get(UserKey, "Usuario");
    public string? GetCommunityId() => Preferences.Get(CommunityKey, null);
    public bool IsAuthenticated() => !string.IsNullOrEmpty(GetToken());

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/reset-password", new { email, newPassword });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ResetPassword Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        try
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token)) return false;

            using var client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsJsonAsync("/api/auth/change-password", new { oldPassword, newPassword });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ChangePassword Exception: {ex.Message}");
            return false;
        }
    }

    public void Logout()
    {
        Preferences.Clear();
    }

    public async Task<(bool Success, string? ErrorMessage)> MeetingAccessLoginAsync(string tokenOrLink, string code)
    {
        try
        {
            var token = ExtractToken(tokenOrLink);
            if (string.IsNullOrWhiteSpace(token)) return (false, "El enlace o token de acceso no es válido.");

            var response = await _httpClient.PostAsJsonAsync("/api/auth/meeting-access", new { token, code });
            if (!response.IsSuccessStatusCode)
            {
                var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                var msg = errorObj != null && errorObj.TryGetValue("error", out var e) ? e : "Credencial de acceso o código incorrecto.";
                return (false, msg);
            }

            var result = await response.Content.ReadFromJsonAsync<MeetingAccessAuthResponse>();
            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                Preferences.Set(TokenKey, result.AccessToken);
                var fullName = result.User != null ? $"{result.User.Name} {result.User.LastName}".Trim() : "Votante";
                Preferences.Set(UserKey, fullName);

                await FetchAndSaveCommunityIdAsync(result.AccessToken);
                return (true, null);
            }

            return (false, "No se pudo obtener la sesión de autenticación.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MeetingAccessLogin Exception: {ex.Message}");
            return (false, "Error de conexión al servidor.");
        }
    }

    private static string ExtractToken(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var trimmed = input.Trim();
        if (trimmed.Contains("/meeting/access/"))
        {
            var parts = trimmed.Split("/meeting/access/");
            return parts.Last().Trim('/');
        }
        return trimmed;
    }
}
