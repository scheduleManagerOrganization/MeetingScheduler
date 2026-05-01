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
            // 🔧 OwnerId 유효성 검사 강화
            if (string.IsNullOrEmpty(request.OwnerId) || request.OwnerId.Length != 24)
            {
                return BadRequest(new { success = false, error = "INVALID_OWNER_ID" });
            }
        
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
            Console.WriteLine($"=== JoinTeam called ===");
            Console.WriteLine($"Request.JoinCode: '{request.JoinCode}'");
            Console.WriteLine($"Request.UserId: '{request.UserId}'");
        
            if (string.IsNullOrEmpty(request.JoinCode))
            {
                return BadRequest(new { success = false, error = "EMPTY_CODE", message = "Join code is required" });
            }
        
            // 🔧 대소문자 구분 없이 검색
            var team = await _mongoDB.Teams
                .Find(x => x.JoinCode.ToUpper() == request.JoinCode.ToUpper())
                .FirstOrDefaultAsync();
        
            if (team == null)
            {
                Console.WriteLine($"❌ Team not found with code: '{request.JoinCode}'");
            
                // 디버깅: 모든 팀의 코드 출력
                var allTeams = await _mongoDB.Teams.Find(_ => true).ToListAsync();
                Console.WriteLine($"Total teams in DB: {allTeams.Count}");
                foreach (var t in allTeams)
                {
                    Console.WriteLine($"  Team: {t.TeamName}, Code: '{t.JoinCode}'");
                }
            
                return NotFound(new { success = false, error = "INVALID_CODE", message = $"Code '{request.JoinCode}' not found" });
            }
        
            Console.WriteLine($"✅ Team found: {team.TeamName}, Id: {team.Id}");
        
            if (team.Members.Any(m => m.UserId == request.UserId))
            {
                return BadRequest(new { success = false, error = "ALREADY_MEMBER" });
            }
        
            var update = Builders<Team>.Update.Push(x => x.Members, new TeamMember
            {
                UserId = request.UserId,
                Role = "member",
                Status = "active",
                JoinedAt = DateTime.UtcNow
            });
        
            await _mongoDB.Teams.UpdateOneAsync(x => x.Id == team.Id, update);
        
            Console.WriteLine($"✅ User {request.UserId} added to team {team.TeamName}");
        
            return Ok(new
            {
                success = true,
                data = new { team_id = team.Id, team_name = team.TeamName }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ Error in JoinTeam: {e.Message}");
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserTeams(string userId)
    {
        try
        {
            Console.WriteLine($"=== GetUserTeams called ===");
            Console.WriteLine($"UserId: '{userId}'");
        
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { success = false, error = "INVALID_USER_ID" });
        
            // 🔧 전체 팀 개수 로깅
            var allTeams = await _mongoDB.Teams.Find(_ => true).ToListAsync();
            Console.WriteLine($"Total teams in DB: {allTeams.Count}");
            foreach (var t in allTeams)
            {
                Console.WriteLine($"  Team: {t.Id}, Members: [{string.Join(", ", t.Members.Select(m => m.UserId))}]");
            }
        
            var teams = await _mongoDB.Teams
                .Find(x => x.Members.Any(m => m.UserId == userId && m.Status == "active"))
                .ToListAsync();
        
            Console.WriteLine($"Found {teams.Count} teams for user {userId}");
        
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
            Console.WriteLine($"❌ Error: {e.Message}");
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
