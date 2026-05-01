using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class SuggestSlotsRequest
{
    [JsonPropertyName("meeting_id")]  // ← JSON 키 이름 지정
    public string MeetingId { get; set; } = string.Empty;
}
