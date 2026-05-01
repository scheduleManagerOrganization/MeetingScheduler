using System.Text;
using MeetingScheduler.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MeetingScheduler.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Process exit logging
AppDomain.CurrentDomain.ProcessExit += (s, e) => 
{
    Console.WriteLine($"Process exiting. Memory used: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
};

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<AuthService>();

// 🔧 JWT Authentication 설정 추가
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"] ?? "meeting-scheduler-secret-key-2026-very-long-and-random-string"))
        };
        
        // 🔧 SignalR을 위한 JWT 설정
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/meetingHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// 🔧 순서 중요: Authentication 먼저!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MeetingHub>("/meetingHub");

// Health check endpoint
app.MapGet("/health", async (MongoDBService db) =>
{
    var isConnected = await db.IsConnectedAsync();
    return Results.Json(new
    {
        status = "healthy",
        mongodb = isConnected ? "connected" : "disconnected",
        timestamp = DateTime.UtcNow
    });
});

app.MapGet("/", () => Results.Json(new
{
    message = "🗓️ AI 미팅 스케줄러 API",
    status = "running",
    websocket = "supported",
    timestamp = DateTime.UtcNow
}));

// Database initialization endpoint
app.MapPost("/api/init-db", async (MongoDBService db) =>
{
    var testUsers = new[]
    {
        new { email = "alice@example.com", password = "alice1234", name = "Alice Kim" },
        new { email = "bob@example.com", password = "bob1234", name = "Bob Park" }
    };
    
    var auth = new AuthService(builder.Configuration);
    
    foreach (var testUser in testUsers)
    {
        var existing = await db.Users.Find(x => x.Email == testUser.email).FirstOrDefaultAsync();
        if (existing == null)
        {
            await db.Users.InsertOneAsync(new MeetingScheduler.Models.User
            {
                Email = testUser.email,
                PasswordHash = auth.HashPassword(testUser.password),
                Name = testUser.name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
    
    return Results.Json(new
    {
        success = true,
        message = "Database initialized",
        data = new { users = testUsers.Select(u => u.email) }
    });
});

// Render용 포트 설정
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();
