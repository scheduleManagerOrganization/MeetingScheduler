using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class TimeSlotDto
{
    [JsonPropertyName("start")]
    public string Start { get; set; } = string.Empty;
    
    [JsonPropertyName("end")]
    public string End { get; set; } = string.Empty;
}

public class SetAvailabilityRequest
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("team_id")]
    public string? TeamId { get; set; }
    
    [JsonPropertyName("slots")]
    public List<TimeSlotDto> Slots { get; set; } = new();
}
