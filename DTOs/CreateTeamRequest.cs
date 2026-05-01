namespace MeetingScheduler.DTOs;

public class CreateTeamRequest
{
    public string TeamName { get; set; } = string.Empty;
    public string? OwnerId { get; set; }  // 🔧 string?으로 변경, 기본값 제거
    public string Description { get; set; } = string.Empty;
}
