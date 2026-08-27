namespace CommunityVoting.Maui.Models;

public record LoginRequest(string Email, string Password);

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Role { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

public class MeetingAccessAuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

public class CommunityItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
