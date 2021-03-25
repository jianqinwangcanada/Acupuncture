using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.CommonFunction.UserSvc;
using Acupuncture.CommonFunction.WritebleAppSettingFunction;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Acupuncture.CommonFunction.AttributeFuction;
using Serilog;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]
    public class EmailSettingController : Controller
    {
        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;
        private readonly IWritebleSettingSvc<SmtpOptions> _writableSvcSmtpOptions;
        private readonly IWritebleSettingSvc<SendGridOptions> _writableSvcSendGridOptions;
        private AdminBaseViewModel _adminBaseViewModel;
        private readonly IWritebleSettingSvc<SiteWideSettings> _writableSiteWideSettings;

       public EmailSettingController(
       IUserSvc userSvc, ICookieSvc cookieSvc, IServiceProvider provider,
       IOptions<DataProtectionKeys> dataProtectionKeys, IOptions<AppSettings> appSettings,
       IWritebleSettingSvc<SmtpOptions> writableSvcSmtpOptions, IWritebleSettingSvc<SendGridOptions> writableSvcSendGridOptions, IWritebleSettingSvc<SiteWideSettings> writableSiteWideSettings)
        {
            _userSvc = userSvc;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            _writableSvcSmtpOptions = writableSvcSmtpOptions;
            _writableSvcSendGridOptions = writableSvcSendGridOptions;
            _writableSiteWideSettings = writableSiteWideSettings;
        }

        public async Task<IActionResult> Index()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIdAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            var protectorSendGrid = protectorProvider.CreateProtector(_dataProtectionKeys.SendGridProtectionKey);
            var dashboard = new DashBoardModel();

            _adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSetting = _appSettings,
                SmtpOption = _writableSvcSmtpOptions.Value,
                SendGridOption = _writableSvcSendGridOptions.Value,
                SiteWideSetting = _writableSiteWideSettings.Value,
                Dashboard = dashboard
            };

            _adminBaseViewModel.SendGridOption.SendGridKey =
                protectorSendGrid.Protect(_adminBaseViewModel.SendGridOption.SendGridKey);

            _adminBaseViewModel.SmtpOption.SmtpPassword =
                protectorSendGrid.Protect(_adminBaseViewModel.SmtpOption.SmtpPassword);

            return View("Index", _adminBaseViewModel);
        }
        [AjaxAccessOnly]
        [HttpPost]
        public IActionResult UpdateSengridOptions([FromBody] SendGridOptions options)
        {
            try
            {
                // Checking to see id key was not updated
                // we dont want the encrypted key from input to be updated
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var decryptedKey = protector.Unprotect(options.SendGridKey);
                options.SendGridKey = decryptedKey;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred decrypting sendgrid key {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            var resultError = _writableSvcSendGridOptions.Update((opt) =>
            {
                opt.FromEmail = options.FromEmail;
                opt.FromFullName = options.FromFullName;
                opt.SendGridKey = options.SendGridKey;
                opt.SendGridUser = options.SendGridUser;
                opt.IsDefault = options.IsDefault;
            });

            if (!resultError)
            {
                if (options.IsDefault)
                {
                    _writableSvcSmtpOptions.Update((optSmtp) => { optSmtp.IsDefault = false; });
                }
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false });
        }

        [AjaxAccessOnly]
        [HttpPost]
        public IActionResult UpdateSmtpOptions([FromBody] SmtpOptions options)
        {
            var resultError = _writableSvcSmtpOptions.Update((opt) =>
            {
                opt.FromEmail = options.FromEmail;
                opt.FromFullName = options.FromFullName;
                opt.SmtpPassword = options.SmtpPassword;
                opt.SmtpUserName = options.SmtpUserName;
                opt.IsDefault = options.IsDefault;
                opt.SmtpPort = options.SmtpPort;
                opt.SmtpSsl = options.SmtpSsl;
                opt.SmtpHost = options.SmtpHost;
            });

            if (!resultError)
            {
                if (options.IsDefault)
                {
                    _writableSvcSendGridOptions.Update((optSend) => { optSend.IsDefault = false; });
                }
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false });
        }


    }
}
