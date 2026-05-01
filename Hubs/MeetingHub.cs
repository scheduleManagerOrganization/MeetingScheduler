using Microsoft.AspNetCore.SignalR;
using MeetingScheduler.Services;
using MongoDB.Driver;

namespace MeetingScheduler.Hubs;

public class MeetingHub : Hub
{
    private readonly MongoDBService _mongoDB;
    
    // 🔧 MongoDB 주입받도록 수정
    public MeetingHub(MongoDBService mongoDB)
    {
        _mongoDB = mongoDB;
    }
    
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
        // 🔧 실제 MeetingId 조회
        var meetingId = await GetSlotMeetingId(slotId);
        if (meetingId != null)
        {
            await Clients.Group($"meeting_{meetingId}").SendAsync("slot_response_updated", new
            {
                slot_id = slotId,
                user_id = userId,
                response = response
            });
        }
    }
    
    // 🔧 실제 MongoDB 조회로 수정
    private async Task<string?> GetSlotMeetingId(string slotId)
    {
        try
        {
            var slot = await _mongoDB.ProposedSlots
                .Find(x => x.Id == slotId)
                .FirstOrDefaultAsync();
            return slot?.MeetingId;
        }
        catch
        {
            return null;
        }
    }
}
