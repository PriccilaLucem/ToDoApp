using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.TaskModel
{

    public class RecurrencePattern
    {
        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public RecurrencePatternType Type { get; set; } = RecurrencePatternType.Daily;

        [BsonElement("interval")]
        [Range(1, 365)]
        public int Interval { get; set; } = 1;

        [BsonElement("daysOfWeek")]
        [BsonRepresentation(BsonType.String)]
        public List<DayOfWeek> DaysOfWeek { get; set; } = new List<DayOfWeek>();

        [BsonElement("endDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        [FutureDateValidation]
        public DateTime? EndDate { get; set; }

        [BsonElement("occurrences")]
        [Range(1, 999)]
        public int? MaxOccurrences { get; set; }

        [BsonElement("exceptions")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public List<DateTime> ExceptionDates { get; set; } = new List<DateTime>();
    }

public class FutureDateValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is DateTime date)
        {
            if (date < DateTime.Now)
            {
                return new ValidationResult("End date must be in the future");
            }
            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid date format");
    }
}
}