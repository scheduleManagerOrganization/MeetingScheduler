using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly MongoDBService _mongoDB;
    
    public TeamsController(MongoDBService mongoDB)
    {
        _mongoDB = mongoDB;
    }
    
    private string GenerateJoinCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        try
        {
            var joinCode = GenerateJoinCode();
            
            var team = new Team
            {
                TeamName = request.TeamName,
                JoinCode = joinCode,
                OwnerId = request.OwnerId,
                Description = request.Description,
                Members = new List<TeamMember>
                {
                    new() { UserId = request.OwnerId, Role = "admin", Status = "active", JoinedAt = DateTime.UtcNow }
                },
                CreatedAt = DateTime.UtcNow
            };
            
            await _mongoDB.Teams.InsertOneAsync(team);
            
            return Ok(new
            {
                success = true,
                data = new { team_id = team.Id, join_code = joinCode }
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpPost("join")]
    public async Task<IActionResult> JoinTeam([FromBody] JoinTeamRequest request)
    {
        try
        {
            var team = await _mongoDB.Teams.Find(x => x.JoinCode == request.JoinCode).FirstOrDefaultAsync();
            if (team == null)
                return NotFound(new { success = false, error = "INVALID_CODE" });
            
            if (team.Members.Any(m => m.UserId == request.UserId))
                return BadRequest(new { success = false, error = "ALREADY_MEMBER" });
            
            team.Members.Add(new TeamMember
            {
                UserId = request.UserId,
                Role = "member",
                Status = "active",
                JoinedAt = DateTime.UtcNow
            });
            
            await _mongoDB.Teams.ReplaceOneAsync(x => x.Id == team.Id, team);
            
            return Ok(new
            {
                success = true,
                data = new { team_id = team.Id, team_name = team.TeamName }
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserTeams(string userId)
    {
        try
        {
            var teams = await _mongoDB.Teams.Find(x => 
                x.Members.Any(m => m.UserId == userId && m.Status == "active")).ToListAsync();
            
            var result = teams.Select(t => new
            {
                team_id = t.Id,
                team_name = t.TeamName,
                join_code = t.JoinCode,
                member_count = t.Members.Count
            });
            
            return Ok(new { success = true, data = result });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
