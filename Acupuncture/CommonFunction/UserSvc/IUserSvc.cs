using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Http;

namespace Acupuncture.CommonFunction.UserSvc
{
    public interface IUserSvc
    {
        Task<ProfileModel> GetUserProfileByIdAsync(string userId);
        Task<ProfileModel> GetUserProfileByUsernameAsync(string username);
        Task<ProfileModel> GetUserProfileByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(ProfileModel model, string password);
        Task<bool> UpdateProfileAsync(IFormCollection formData);
        Task<bool> ChangePasswordAsync(ProfileModel model, string newPassword);
        Task<bool> AddUserActivity(Activity model);


        Task<List<Activity>> GetUserActivity(string username);
    }
}
