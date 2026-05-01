using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class CreateTeamRequest
{
    [JsonPropertyName("team_name")]
    public string TeamName { get; set; } = string.Empty;
    
    [JsonPropertyName("owner_id")]  // 🔧 추가
    public string? OwnerId { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
