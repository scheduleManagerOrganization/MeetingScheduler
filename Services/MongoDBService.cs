using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MeetingScheduler.Models;

namespace MeetingScheduler.Services;

public class MongoDBService
{
    private readonly IMongoDatabase _database;
    
    public MongoDBService(IConfiguration config)
    {
        var connectionString = config["MONGODB_URI"] ?? 
            config.GetConnectionString("MongoDB");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("meeting_scheduler");
    }
    
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Team> Teams => _database.GetCollection<Team>("teams");
    public IMongoCollection<Meeting> Meetings => _database.GetCollection<Meeting>("meetings");
    public IMongoCollection<UserCalendar> UserCalendars => _database.GetCollection<UserCalendar>("user_calendars");
    public IMongoCollection<ProposedSlot> ProposedSlots => _database.GetCollection<ProposedSlot>("proposed_slots");
    public IMongoCollection<ParticipantResponse> ParticipantResponses => _database.GetCollection<ParticipantResponse>("participant_responses");
    
    // 데이터베이스 연결 확인
    public async Task<bool> IsConnectedAsync()
    {
        try
        {
            await _database.RunCommandAsync((Command<object>)"{ping:1}");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
