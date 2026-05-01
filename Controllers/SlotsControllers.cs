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
            if (request == null)
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw request body: {body}");
                return BadRequest(new { success = false, error = "INVALID_REQUEST" });
            }
            
            var meetingId = request.MeetingId;
            
            if (string.IsNullOrWhiteSpace(meetingId) || meetingId.Length != 24 || 
                !System.Text.RegularExpressions.Regex.IsMatch(meetingId, "^[0-9a-fA-F]{24}$"))
            {
                return BadRequest(new { 
                    success = false, 
                    error = "INVALID_MEETING_ID",
                    message = $"Meeting ID must be a 24-character hex string." 
                });
            }
            
            var meeting = await _mongoDB.Meetings.Find(x => x.Id == meetingId).FirstOrDefaultAsync();
            if (meeting == null)
                return NotFound(new { success = false, error = "MEETING_NOT_FOUND" });
            
            var team = await _mongoDB.Teams.Find(x => x.Id == meeting.TeamId).FirstOrDefaultAsync();
            if (team == null)
                return NotFound(new { success = false, error = "TEAM_NOT_FOUND" });
            
            if (team.Members == null || !team.Members.Any())
                return BadRequest(new { success = false, error = "NO_TEAM_MEMBERS" });
            
            var memberIds = team.Members.Select(m => m.UserId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            
            // 🔧 날짜 범위를 미리 리스트로 생성
            var startDate = DateTime.UtcNow;
            var dateRange = new List<string>();
            for (int d = 0; d < 7; d++)
            {
                dateRange.Add(startDate.AddDays(d).ToString("yyyy-MM-dd"));
            }
            
            // 🔧 string.Compare 대신 dateRange.Contains 사용 (MongoDB $in 쿼리로 변환 가능)
            var availabilities = await _mongoDB.UserCalendars
                .Find(x => memberIds.Contains(x.UserId) && dateRange.Contains(x.Date))
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
                        suggestedSlots.Add(new ProposedSlot
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            MeetingId = meeting.Id,
                            StartTime = startTime,
                            EndTime = endTime,
                            AiScore = Math.Round((double)availableCount / memberIds.Count * 100, 2),
                            Responses = new List<SlotResponse>(),
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            
            if (suggestedSlots.Any())
                await _mongoDB.ProposedSlots.InsertManyAsync(suggestedSlots);
            
            return Ok(new { success = true, data = new { suggested_count = suggestedSlots.Count } });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
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
                return BadRequest(new { success = false, error = "INVALID_SLOT_ID" });
            
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
