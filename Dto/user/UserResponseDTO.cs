namespace WebApplication.Dto.user
{
    public class UserResponseDTO 
    {
    public required string Id { get; set;} 
    public required string Name { get; set;}
    public required DateTime BirthDate{ get; set; }

    public required string Email{ get; set;}
    }
}