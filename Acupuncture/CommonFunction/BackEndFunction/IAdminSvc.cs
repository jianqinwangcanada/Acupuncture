using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Http;

namespace Acupuncture.CommonFunction.BackEndFunction
{
    public interface IAdminSvc
    {
        Task<ProfileModel> GetUserProfileByUsernameAsync(string username);
        Task<List<UserModel>> GetAllUsersAsync();
        Task<bool> UpdateProfileAsync(string userId, IFormCollection formData);
        Task<Dictionary<string, List<string>>> AddUserAsync(ApplicationUser user, string password);
        Task<List<string>> ResetPasswordAsync(string username);
        Task<bool> DeleteUserAsync(string username);
    }
}
