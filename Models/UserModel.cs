using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRequired]
        public required string Name { get; set; } 

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        [EmailAddress]
        public required string Email { get; set; }

        [BsonRequired]
        [StringLength(300, MinimumLength = 6)]
        public required string  Password { get; set; }

        [BsonRequired]
        public bool Status { get; set; } = true;
        [BsonRequired]
        public required DateTime BirthDate{ get; set; }

        public required DateTime CreatedAt { get; set; } = DateTime.Now;
        public required DateTime UpdatedAt{ get; set;}
    }
}
