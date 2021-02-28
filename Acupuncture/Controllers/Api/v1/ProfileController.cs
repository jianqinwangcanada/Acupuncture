using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.CommonFunction.UserSvc;
using Acupuncture.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Controllers.Api.v1
{   [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ICookieSvc _cookieSvc;
            private readonly IUserSvc _userSvc;
        public ProfileController(ICookieSvc cookieSvc,IUserSvc userSvc)
        {
            _cookieSvc = cookieSvc;
            _userSvc = userSvc;
        }
        [HttpGet("[action]/{username}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] string username)
        {
            if (username == null) return BadRequest();
            var result = await _userSvc.GetUserProfileByUsernameAsync(username);
            if (result == null) return NotFound();
            return Ok(result);

        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateProfile(IFormCollection formData)
        {
            ProfileModel model = new ProfileModel { Username = formData["Username"] };

            var password = formData["Password"].ToString();

            if (await _userSvc.CheckPasswordAsync(model, password))
            {
                var result = await _userSvc.UpdateProfileAsync(formData);

                if (result)
                {
                    return Ok(new { Message = "Profile updated Successfully!" });
                }
            }

            return BadRequest(new { Message = "Could not Update Profile." });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.OldPassword))
            {
                return BadRequest("Old Password must be supplied for password change.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }

            var user = await _userSvc.GetUserProfileByEmailAsync(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok(new { message = "Password changed Successfully" });
            }

            if (!await _userSvc.CheckPasswordAsync(user, model.OldPassword))
            {
                // Notify attempt was made - to change password failed
                Activity activityModel = new Activity
                {
                    UserId = user.UserId,
                    Date = DateTime.UtcNow,
                    IpAddress = _cookieSvc.GetUserIP(),
                    Location = _cookieSvc.GetUserCountry(),
                    OperatingSystem = _cookieSvc.GetUserOS(),
                    Type = "Profile update failed - Invalid Old Password",
                    Icon = "fas fa-exclamation-triangle",
                    Color = "warning"
                };

                var activityAdd = await _userSvc.AddUserActivity(activityModel);

                return BadRequest(new { message = "Invalid Old Password" });
            }

            var result = await _userSvc.ChangePasswordAsync(user, model.Password);

            if (result)
            {
                return Ok(new { message = "Password changed Successfully" });
            }
            return BadRequest(new { message = "Password could not be Changed. Try again later" });
        }

        [HttpGet("[action]/{username}")]
        public async Task<IActionResult> GetUserActivity([FromRoute] string username)
        {
            var result = await _userSvc.GetUserActivity(username);

            if (result != null)
            {
                return Ok(new { Message = "Fetched user activities successfully!", data = result });
            }

            return BadRequest(new { Message = "Could not fetch user activities." });
        }
    }
}
