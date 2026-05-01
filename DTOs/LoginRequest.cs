using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
