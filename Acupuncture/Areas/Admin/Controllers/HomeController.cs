using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.CommonFunction.UserSvc;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Areas.Admin.Controllers
{   [Area("Admin")]
    [Authorize(AuthenticationSchemes ="Admin")]
    public class HomeController : Controller
    {
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly ICookieSvc _cookieSvc;
        private static AdminBaseViewModel _adminBaseViewModel;
        //The following used for Userservice
        private readonly IUserSvc _userSvc;
        public HomeController(IServiceProvider provider, IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionKeys, ICookieSvc cookieSvc, IUserSvc userSvc)
        {
            _provider = provider;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _cookieSvc = cookieSvc;
            //_userSvc = new UserSvc(provider, activitySvc, cookieSvc, db, dataProtectionKeys, ev,userManager);
            _userSvc = userSvc;
        }

        public async Task<IActionResult> Index()
        {
            await SetBaseViewModel();
            return View("index",_adminBaseViewModel);
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
                AddUser = null,
                Dashboard = null,
                AppSetting = null,
                SendGridOption = null,
                SmtpOption = null,
                ResetPassword = null,
                SiteWideSetting = null

            };

        }
    }
}
