using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("api/meetings")]
public class MeetingsController : ControllerBase
{
    private readonly MongoDBService _mongoDB;
    
    public MeetingsController(MongoDBService mongoDB)
    {
        _mongoDB = mongoDB;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
    {
        try
        {
            var meeting = new Meeting
            {
                TeamId = request.TeamId,
                Title = request.Title,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                CreatorId = request.CreatorId,
                Status = "proposing",
                DeadlineDate = request.DeadlineDate,
                CreatedAt = DateTime.UtcNow
            };
            
            await _mongoDB.Meetings.InsertOneAsync(meeting);
            
            return Ok(new
            {
                success = true,
                data = new { meeting_id = meeting.Id }
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("{meetingId}")]
    public async Task<IActionResult> GetMeeting(string meetingId)
    {
        try
        {
            var meeting = await _mongoDB.Meetings.Find(x => x.Id == meetingId).FirstOrDefaultAsync();
            if (meeting == null)
                return NotFound(new { success = false, error = "MEETING_NOT_FOUND" });
            
            return Ok(new { success = true, data = meeting });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("team/{teamId}")]
    public async Task<IActionResult> GetTeamMeetings(string teamId)
    {
        try
        {
            var meetings = await _mongoDB.Meetings
                .Find(x => x.TeamId == teamId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
            
            return Ok(new { success = true, data = meetings });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
