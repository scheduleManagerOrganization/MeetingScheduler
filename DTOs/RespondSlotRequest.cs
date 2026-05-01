using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class RespondSlotRequest
{
    [JsonPropertyName("slot_id")]
    public string SlotId { get; set; } = string.Empty;
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}
