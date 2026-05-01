namespace MeetingScheduler.DTOs;

public class CreateTeamRequest
{
    public string TeamName { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
