using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication.Src.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        [BsonElement("email")]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [BsonElement("password")]
        [Required]
        public string? Password { get; set; }

        [BsonElement("birthDate")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/1900", "1/1/2024")]
        public DateTime BirthDate { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();
        [BsonElement("status")]
        public bool? Status { get; set; } = true;

        public static ValidationResult ValidateTimestamps(DateTime updatedAt, ValidationContext context)
        {
            var model = (UserModel)context.ObjectInstance;
            return updatedAt < model.CreatedAt 
                ? new ValidationResult("UpdatedAt cannot be before CreatedAt") 
                : ValidationResult.Success;
        }
    }
}