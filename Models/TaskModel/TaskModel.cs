using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace WebApplication.Models.TaskModel
{
    public class TaskModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("title")]
        [BsonRequired]
        public string Title { get; set; } = string.Empty; 

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty; 

        [BsonElement("category")]
        public string Category { get; set; } = "Personal Care"; 

        [BsonElement("dueDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DueDate { get; set; }

        [BsonElement("priority")]
        public int Priority { get; set; } = 3; 

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; } = 15; 

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>(); 

        [BsonElement("recurrence")]
        public RecurrencePattern? Recurrence { get; set; } 

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty; 
    }
}