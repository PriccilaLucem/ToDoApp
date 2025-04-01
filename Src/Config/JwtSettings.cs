
namespace WebApplication.Src.Config
{
    public class JwtSettings(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

                
        public string SecretKey => GetSecretKey();

        private string GetSecretKey()
        {
            var key = _configuration["JwtSettings:SecretKey"] 
                    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException(
                    "JWT secret key not configured. " +
                    "Set it in appsettings.json or environment variables.");
            }

            //TODO remove it later
            if (key.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT secret key must be at least 32 characters long.");
            }

            return key;
        }
    }
}