namespace MeetingScheduler.DTOs;

public class JoinTeamRequest
{
    public string join_code { get; set; } = string.Empty;  // ← 소문자 + 언더스코어
    public string user_id { get; set; } = string.Empty;    // ← 소문자 + 언더스코어
}
