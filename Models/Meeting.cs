using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeetingScheduler.Models;

public class Meeting
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("team_id")]
    public string TeamId { get; set; } = string.Empty;
    
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
    
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
    
    [BsonElement("duration_minutes")]
    public int DurationMinutes { get; set; }
    
    [BsonElement("creator_id")]
    public string CreatorId { get; set; } = string.Empty;
    
    [BsonElement("status")]
    public string Status { get; set; } = "proposing";
    
    [BsonElement("deadline_date")]
    public string? DeadlineDate { get; set; }
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("finalized_slot_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FinalizedSlotId { get; set; }
}
