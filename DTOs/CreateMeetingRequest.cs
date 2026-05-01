namespace MeetingScheduler.DTOs;

public class CreateMeetingRequest
{
    public string TeamId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string? DeadlineDate { get; set; }
}
