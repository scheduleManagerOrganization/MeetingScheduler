namespace MeetingScheduler.DTOs;

public class JoinTeamRequest
{
    public string JoinCode { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
