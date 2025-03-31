using static BCrypt.Net.BCrypt;

namespace WebApplication.Util
{
    
    public class Auth
    {
    private const int WorkFactor = 12;
    public static string HashPasswordUtil(string password)
    {
    string hashedPassword = HashPassword(password, WorkFactor);
    return hashedPassword;
    }

    public  static bool VerifyPasswordUtil(string password, string hashedPassword)
    {
        if (Verify(password, hashedPassword))
        {
             return true; 
        }
        return false;
    }
    }
}