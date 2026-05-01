using System.Text.Json.Serialization;

namespace MeetingScheduler.DTOs;

public class SuggestSlotsRequest
{
    public string MeetingId { get; set; } = string.Empty;
}
