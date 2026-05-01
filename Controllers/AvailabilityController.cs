using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly MongoDBService _mongoDB;
    
    public AvailabilityController(MongoDBService mongoDB)
    {
        _mongoDB = mongoDB;
    }
    
    [HttpPost]
    public async Task<IActionResult> SetAvailability([FromBody] SetAvailabilityRequest request)
    {
        try
        {
            // 업데이트할 필드만 지정
            var update = Builders<UserCalendar>.Update
                .Set(x => x.UserId, request.UserId)
                .Set(x => x.Date, request.Date)
                .Set(x => x.Slots, request.Slots.Select(s => new TimeSlot { Start = s.Start, End = s.End }).ToList())
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
        
            // Upsert: 있으면 업데이트, 없으면 생성
            var result = await _mongoDB.UserCalendars.UpdateOneAsync(
                x => x.UserId == request.UserId && x.Date == request.Date,
                update,
                new UpdateOptions { IsUpsert = true }
            );
        
            return Ok(new { success = true, message = "일정이 저장되었습니다" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("{userId}/{date}")]
    public async Task<IActionResult> GetAvailability(string userId, string date)
    {
        try
        {
            var availability = await _mongoDB.UserCalendars
                .Find(x => x.UserId == userId && x.Date == date)
                .FirstOrDefaultAsync();
            
            return Ok(new { success = true, data = availability });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("team/{teamId}/{date}")]
    public async Task<IActionResult> GetTeamAvailability(string teamId, string date)
    {
        try
        {
            var team = await _mongoDB.Teams.Find(x => x.Id == teamId).FirstOrDefaultAsync();
            if (team == null)
                return NotFound(new { success = false, error = "TEAM_NOT_FOUND" });
            
            var memberIds = team.Members.Select(m => m.UserId).ToList();
            
            var availabilities = await _mongoDB.UserCalendars
                .Find(x => memberIds.Contains(x.UserId) && x.Date == date)
                .ToListAsync();
            
            var users = await _mongoDB.Users
                .Find(x => memberIds.Contains(x.Id))
                .ToListAsync();
            
            var userDict = users.ToDictionary(u => u.Id, u => u.Name);
            
            var result = availabilities.Select(a => new
            {
                user_id = a.UserId,
                user_name = userDict.GetValueOrDefault(a.UserId, "Unknown"),
                slots = a.Slots
            });
            
            return Ok(new { success = true, data = result });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
