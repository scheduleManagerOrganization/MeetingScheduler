using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class SuggestSlotsRequest
{
    [JsonPropertyName("meeting_id")]
    public string MeetingId { get; set; } = string.Empty;
}
