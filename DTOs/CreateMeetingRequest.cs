using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class CreateMeetingRequest
{
    [JsonPropertyName("team_id")]
    public string? TeamId { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("duration_minutes")]
    public int DurationMinutes { get; set; }
    
    [JsonPropertyName("creator_id")]
    public string? CreatorId { get; set; }
    
    [JsonPropertyName("deadline_date")]
    public string? DeadlineDate { get; set; }
}
