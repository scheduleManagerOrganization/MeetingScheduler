using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;          
using MongoDB.Bson;             
using MeetingScheduler.Services;
using MeetingScheduler.DTOs;
using MeetingScheduler.Models;

namespace MeetingScheduler.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly MongoDBService _mongoDB;
    private readonly AuthService _auth;
    
    public AuthController(MongoDBService mongoDB, AuthService auth)
    {
        _mongoDB = mongoDB;
        _auth = auth;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // 이메일 중복 확인
            var existing = await _mongoDB.Users.Find(x => x.Email == request.Email).FirstOrDefaultAsync();
            if (existing != null)
                return BadRequest(new { success = false, error = "EMAIL_EXISTS" });
            
            var user = new User
            {
                Email = request.Email,
                PasswordHash = _auth.HashPassword(request.Password),
                Name = request.Name,
                Timezone = request.Timezone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _mongoDB.Users.InsertOneAsync(user);
            
            var token = _auth.GenerateJwtToken(user.Id, user.Email, user.Name);

            Console.WriteLine($"User Id: {user.Id}");
            Console.WriteLine($"User Id length: {user.Id.Length}");
        
            return Ok(new
            {
                success = true,
                data = new { user_id = user.Id, name = user.Name, email = user.Email, token }
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _mongoDB.Users.Find(x => x.Email == request.Email).FirstOrDefaultAsync();
            if (user == null || !_auth.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { success = false, error = "INVALID_CREDENTIALS" });
            
            var token = _auth.GenerateJwtToken(user.Id, user.Email, user.Name);
            
            return Ok(new
            {
                success = true,
                data = new { user_id = user.Id, name = user.Name, email = user.Email, token }
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, error = e.Message });
        }
    }
}
