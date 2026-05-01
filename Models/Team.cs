using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeetingScheduler.Models;

public class TeamMember
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;
    
    public string Role { get; set; } = "member";
    public string Status { get; set; } = "active";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Team
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("team_name")]
    public string TeamName { get; set; } = string.Empty;
    
    [BsonElement("join_code")]
    public string JoinCode { get; set; } = string.Empty;
    
    [BsonElement("owner_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerId { get; set; } = string.Empty;
    
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
    
    [BsonElement("members")]
    public List<TeamMember> Members { get; set; } = new();
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
