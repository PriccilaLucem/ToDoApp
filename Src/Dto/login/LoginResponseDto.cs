namespace WebApplication.Src.Dto.login
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public required int ExpiresIn { get; set; }
        public required string UserId { get; set; }
        public required string Email { get; set; }
    }
}