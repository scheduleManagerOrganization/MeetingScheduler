using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeetingScheduler.Models;

public class ParticipantResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("slot_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SlotId { get; set; }
    
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }
    
    [BsonElement("response")]
    public string Response { get; set; } = string.Empty;
    
    [BsonElement("responded_at")]
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
