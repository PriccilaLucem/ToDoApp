using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Src.Interface;
using WebApplication.Src.Models;

namespace WebApplication.WebApplication.Tests.Interface
{
    public interface IFakeUserView : IUserView
    {
        Task<List<UserModel>> GetUsersAsync();
        Task<string> CreateUserAsync(UserModel user);
        Task<bool> DeleteUserAsync(string id);
        Task<UserModel> UpdateUserAsync(UserModel user);
        new Task<UserModel?> GetUserByIdAsync(string id);
    }
}