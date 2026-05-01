using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = "Asia/Seoul";
}
