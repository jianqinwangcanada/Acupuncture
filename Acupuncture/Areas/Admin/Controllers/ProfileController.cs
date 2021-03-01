using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Acupuncture.CommonFunction.UserSvc;
using Microsoft.AspNetCore.Authorization;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]

    public class ProfileController : Controller
    {
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly ICookieSvc _cookieSvc;
        private static AdminBaseViewModel _adminBaseViewModel;
        //The following used for Userservice
        private readonly IUserSvc _userSvc;
        //private readonly IActivitySvc activitySvc;
        //private readonly ApplicationDbContext db;
        //private readonly IHostingEnvironment env;
        //private readonly UserManager<ApplicationUser> userManager;

        public ProfileController(IServiceProvider provider,IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionKeys,ICookieSvc cookieSvc,IUserSvc userSvc)
        {
            _provider = provider;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _cookieSvc = cookieSvc;
            //_userSvc = new UserSvc(provider, activitySvc, cookieSvc, db, dataProtectionKeys, ev,userManager);
            _userSvc = userSvc;
        } 


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetBaseViewModel();
            return View("Index",_adminBaseViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Security()
        {
            await SetBaseViewModel();
            return View("Security", _adminBaseViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Activity()
        {
            await SetBaseViewModel();
            return View("Activity", _adminBaseViewModel);
        }
        private async Task SetBaseViewModel()
        {
            var protectedUid = _cookieSvc.Get("user_id");
            //If here is some problems , we should using denpendency injection
            var protectProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var unprotectedUID = protector.Unprotect(protectedUid);
            var userProfile = await _userSvc.GetUserProfileByIdAsync(unprotectedUID);
            var resetPassword = new ResetPasswordViewModel();
            _adminBaseViewModel = new AdminBaseViewModel()
            {
                Profile = userProfile,
                AddUser =null,
                 Dashboard =null,
                AppSetting=null,
                SendGridOption =null,
                 SmtpOption=null,
                 ResetPassword =null,
               SiteWideSetting =null

    };

        }
    }
}
