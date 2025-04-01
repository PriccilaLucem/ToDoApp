using System.ComponentModel.DataAnnotations;

namespace WebApplication.Src.Dto.User
{
    public class CreateUserDTO
    {
    [Required]
    [EmailAddress]
    public required string  Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }
    
    public required string Name { get; set;}
    public required DateTime BirthDate{ get; set; }

    }
}