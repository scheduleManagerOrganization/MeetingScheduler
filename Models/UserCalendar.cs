using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MeetingScheduler.Models;

public class TimeSlot
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
}

public class UserCalendar
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    [BsonElement("date")]
    public string Date { get; set; } = string.Empty;
    
    [BsonElement("slots")]
    public List<TimeSlot> Slots { get; set; } = new();
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
