// Program.cs 맨 위에 추가
AppDomain.CurrentDomain.ProcessExit += (s, e) => 
{
    Console.WriteLine($"Process exiting. Memory used: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
};

using MeetingScheduler.Services;
using MeetingScheduler.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<AuthService>();

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
    // 테스트 사용자 생성
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

app.Run();
