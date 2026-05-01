using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("api")]
public class SlotsController : ControllerBase
{
    private readonly MongoDBService _mongoDB;
    
    public SlotsController(MongoDBService mongoDB)
    {
        _mongoDB = mongoDB;
    }
    
    [HttpPost("suggest-slots")]
    public async Task<IActionResult> SuggestSlots([FromBody] SuggestSlotsRequest request)
    {
        try
        {
            // 🔧 1. MeetingId 유효성 검증
            if (string.IsNullOrEmpty(request.MeetingId) || request.MeetingId.Length != 24)
            {
                return BadRequest(new { success = false, error = "INVALID_MEETING_ID", message = "Meeting ID must be a 24-character hex string" });
            }
            
            var meeting = await _mongoDB.Meetings.Find(x => x.Id == request.MeetingId).FirstOrDefaultAsync();
            if (meeting == null)
                return NotFound(new { success = false, error = "MEETING_NOT_FOUND" });
            
            // 🔧 2. TeamId 유효성 검증
            if (string.IsNullOrEmpty(meeting.TeamId) || meeting.TeamId.Length != 24)
            {
                return BadRequest(new { success = false, error = "INVALID_TEAM_ID", message = "Team ID is invalid" });
            }
            
            var team = await _mongoDB.Teams.Find(x => x.Id == meeting.TeamId).FirstOrDefaultAsync();
            if (team == null)
                return NotFound(new { success = false, error = "TEAM_NOT_FOUND" });
            
            if (team.Members == null || !team.Members.Any())
            {
                return BadRequest(new { success = false, error = "NO_TEAM_MEMBERS", message = "Team has no members" });
            }
            
            var memberIds = team.Members.Select(m => m.UserId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            
            var availabilities = await _mongoDB.UserCalendars
                .Find(x => memberIds.Contains(x.UserId) && 
                           string.Compare(x.Date, startDate.ToString("yyyy-MM-dd")) >= 0 &&
                           string.Compare(x.Date, endDate.ToString("yyyy-MM-dd")) <= 0)
                .ToListAsync();
            
            var suggestedSlots = new List<ProposedSlot>();
            
            for (int day = 0; day < 7; day++)
            {
                var currentDate = startDate.AddDays(day).ToString("yyyy-MM-dd");
                
                foreach (var hour in new[] { 10, 14, 16 })
                {
                    var startTime = DateTime.Parse($"{currentDate} {hour}:00");
                    var endTime = startTime.AddMinutes(meeting.DurationMinutes);
                    
                    var availableCount = 0;
                    foreach (var av in availabilities)
                    {
                        if (av.Date == currentDate && av.Slots != null)
                        {
                            foreach (var slot in av.Slots)
                            {
                                if (slot != null && !string.IsNullOrEmpty(slot.Start))
                                {
                                    var startHour = int.Parse(slot.Start.Split(':')[0]);
                                    var endHour = int.Parse(slot.End.Split(':')[0]);
                                    if (startHour <= hour && hour < endHour)
                                    {
                                        availableCount++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    
                    if (availableCount > 0 && memberIds.Count > 0)
                    {
                        // 🔧 3. 새로운 ObjectId 생성 (빈 문자열 방지)
                        var slot = new ProposedSlot
                        {
                            Id = ObjectId.GenerateNewId().ToString(),  // 명시적으로 생성
                            MeetingId = meeting.Id,
                            StartTime = startTime,
                            EndTime = endTime,
                            AiScore = Math.Round((double)availableCount / memberIds.Count * 100, 2),
                            IsFinalized = false,
                            Responses = new List<SlotResponse>(),
                            CreatedAt = DateTime.UtcNow
                        };
                        suggestedSlots.Add(slot);
                    }
                }
            }
            
            if (suggestedSlots.Any())
            {
                await _mongoDB.ProposedSlots.InsertManyAsync(suggestedSlots);
            }
            
            return Ok(new
            {
                success = true,
                data = new { suggested_count = suggestedSlots.Count }
            });
        }
        catch (FormatException fe)
        {
            return StatusCode(500, new { success = false, error = "FORMAT_ERROR", message = fe.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message, stack = e.StackTrace });
        }
    }
    
    [HttpGet("slots/{meetingId}")]
    public async Task<IActionResult> GetSlots(string meetingId)
    {
        try
        {
            if (string.IsNullOrEmpty(meetingId))
                return BadRequest(new { success = false, error = "INVALID_MEETING_ID" });
            
            var slots = await _mongoDB.ProposedSlots
                .Find(x => x.MeetingId == meetingId)
                .ToListAsync();
            
            return Ok(new { success = true, data = slots });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpPost("respond-slot")]
    public async Task<IActionResult> RespondToSlot([FromBody] RespondSlotRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SlotId) || request.SlotId.Length != 24)
            {
                return BadRequest(new { success = false, error = "INVALID_SLOT_ID" });
            }
            
            var slot = await _mongoDB.ProposedSlots.Find(x => x.Id == request.SlotId).FirstOrDefaultAsync();
            if (slot == null)
                return NotFound(new { success = false, error = "SLOT_NOT_FOUND" });
            
            var filter = Builders<ProposedSlot>.Filter.Eq(x => x.Id, request.SlotId);
            
            var update = Builders<ProposedSlot>.Update
                .PullFilter(x => x.Responses, r => r.UserId == request.UserId)
                .Push(x => x.Responses, new SlotResponse
                {
                    UserId = request.UserId,
                    Response = request.Response,
                    RespondedAt = DateTime.UtcNow
                });
            
            await _mongoDB.ProposedSlots.UpdateOneAsync(filter, update);
            
            return Ok(new { success = true, message = "응답이 저장되었습니다" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
