using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeetingScheduler.Models;

public class SlotResponse
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;
    
    public string Response { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}

public class ProposedSlot
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("meeting_id")]
    public string MeetingId { get; set; } = string.Empty;
    
    [BsonElement("start_time")]
    public DateTime StartTime { get; set; }
    
    [BsonElement("end_time")]
    public DateTime EndTime { get; set; }
    
    [BsonElement("ai_score")]
    public double AiScore { get; set; }
    
    [BsonElement("is_finalized")]
    public bool IsFinalized { get; set; } = false;
    
    [BsonElement("responses")]
    public List<SlotResponse> Responses { get; set; } = new();
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
