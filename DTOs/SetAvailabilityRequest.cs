namespace MeetingScheduler.DTOs;

public class TimeSlotDto
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class SetAvailabilityRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public List<TimeSlotDto> Slots { get; set; } = new();
}
