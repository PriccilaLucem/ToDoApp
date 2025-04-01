using System.ComponentModel.DataAnnotations;

namespace WebApplication.Src.Dto.task
{
    public class TaskDTO
    {

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = "Personal Care";

        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Range(1, 5, ErrorMessage = "Priority must be between 1 (highest) and 5 (lowest)")]
        public int Priority { get; set; } = 3;

        public bool IsCompleted { get; set; } = false;

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours)")]
        public int? DurationMinutes { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public RecurrencePatternDTO? Recurrence { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;
    }

    public class RecurrencePatternDTO
    {
        [Required]
        public RecurrencePatternType Type { get; set; } = RecurrencePatternType.Daily;

        [Range(1, 365, ErrorMessage = "Interval must be between 1 and 365")]
        public int Interval { get; set; } = 1;

        public List<DayOfWeek> DaysOfWeek { get; set; } = new List<DayOfWeek>();

        [FutureDateValidation]
        public DateTime? EndDate { get; set; }

        [Range(1, 999, ErrorMessage = "Max occurrences must be between 1 and 999")]
        public int? MaxOccurrences { get; set; }

        public List<DateTime> ExceptionDates { get; set; } = new List<DateTime>();
    }

    public class FutureDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            if (value is DateTime date && date < DateTime.Now)
                return new ValidationResult("End date must be in the future");

            return ValidationResult.Success;
        }
    }
}