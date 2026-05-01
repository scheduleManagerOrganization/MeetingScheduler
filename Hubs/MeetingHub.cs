using Microsoft.AspNetCore.SignalR;

namespace MeetingScheduler.Hubs;

public class MeetingHub : Hub
{
    public async Task JoinTeamRoom(string teamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
        await Clients.Caller.SendAsync("joined", new { room = $"team_{teamId}", message = "팀 방에 입장했습니다" });
    }
    
    public async Task JoinMeetingRoom(string meetingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"meeting_{meetingId}");
        await Clients.Caller.SendAsync("joined", new { room = $"meeting_{meetingId}", message = "미팅 방에 입장했습니다" });
    }
    
    public async Task SendMessage(string room, string message, string userName)
    {
        await Clients.Group(room).SendAsync("receive_message", new
        {
            user_name = userName,
            message = message,
            timestamp = DateTime.UtcNow.ToString("o")
        });
    }
    
    public async Task NotifyAvailabilityUpdated(string userId, string date, string teamId)
    {
        await Clients.Group($"team_{teamId}").SendAsync("availability_updated", new
        {
            user_id = userId,
            date = date,
            team_id = teamId
        });
    }
    
    public async Task NotifyMeetingCreated(string meetingId, string title, string teamId)
    {
        await Clients.Group($"team_{teamId}").SendAsync("meeting_created", new
        {
            meeting_id = meetingId,
            title = title
        });
    }
    
    public async Task NotifySlotResponseUpdated(string slotId, string userId, string response)
    {
        var slot = await GetSlotMeetingId(slotId);
        if (slot != null)
        {
            await Clients.Group($"meeting_{slot}").SendAsync("slot_response_updated", new
            {
                slot_id = slotId,
                user_id = userId,
                response = response
            });
        }
    }
    
    private async Task<string?> GetSlotMeetingId(string slotId)
    {
        // 실제 구현에서는 MongoDB 조회 필요
        return slotId;
    }
}
