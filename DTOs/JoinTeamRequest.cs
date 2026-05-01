using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class JoinTeamRequest
{
    [JsonPropertyName("join_code")]
    public string JoinCode { get; set; } = string.Empty;
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
}
