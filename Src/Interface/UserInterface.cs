using WebApplication.Src.Models;
namespace WebApplication.Src.Interface
{
    public interface IUserInterface
    {
    Task<List<UserModel>> GetUsers();
    Task<string> CreateUsers(UserModel UserModel);
    Task<bool> DeleteUsers(string id);
    Task<UserModel> UpdateUsers(UserModel user);
}
}