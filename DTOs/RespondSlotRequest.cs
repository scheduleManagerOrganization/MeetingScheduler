namespace MeetingScheduler.DTOs;

public class RespondSlotRequest
{
    public string SlotId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Response { get; set; } = string.Empty;  // 'yes', 'no', 'maybe'
}
