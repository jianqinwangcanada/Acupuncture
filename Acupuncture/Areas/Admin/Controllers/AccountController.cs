using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.AuthFunction;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Areas.Admin.Controllers
{   [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AppSettings _appSettings;
        private DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        private readonly ApplicationDbContext _db;
        private readonly IAuthenticateSvc _authenticateSvc;
        private readonly ICookieSvc _cookieSvc;
        private const string AccessToken = "access_token";
        private const string User_Id = "user_id";
       public string[] cookiesToDelete = {"twoFactorToken","memberId" ,"rememberDevice","user_id","access_token"};
        public AccountController(IOptions<AppSettings> appSettings,ApplicationDbContext db,
            IAuthenticateSvc authenticateSvc,ICookieSvc cookieSvc,IServiceProvider provider,IOptions<DataProtectionKeys> dataProtectionKey)
        {
            _appSettings = appSettings.Value;
            _db = db;
            _provider = provider;
            _authenticateSvc = authenticateSvc;
            _dataProtectionKeys = dataProtectionKey.Value;
            _cookieSvc = cookieSvc;

        }





        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model,string redirectURL=null)
        {
            ViewData["ReturnUrl"] = redirectURL;
            if (ModelState.IsValid)
            {
                try
                {
                    var jwtTokenResponse = await _authenticateSvc.Auth(model);
                    const int expireTime = 60;
                    _cookieSvc.SetCookie(AccessToken, jwtTokenResponse.Token, expireTime);
                    _cookieSvc.SetCookie(User_Id, jwtTokenResponse.UserId, expireTime);
                    _cookieSvc.SetCookie("User_Name", jwtTokenResponse.Username, expireTime);
                    Log.Information($"User {model.Email} :Login success");
                    return Ok("success");


                }
                catch (Exception ex)
                {
                    Log.Error("Error while Login Post with user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                }
            }
            ModelState.AddModelError("","Invaid UserName/Password was entered");
            Log.Error("Invaid UserName / Password was entered");
            return Unauthorized("Please Check UserNAME/Password");
            
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var user_id = _cookieSvc.Get(User_Id);
                if (user_id != null)
                {
                    var protectProvider = _provider.GetService<IDataProtectionProvider>();
                    var protector = protectProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                    var unprotectedUserId = protector.Unprotect(user_id);
                    var refreshToken = _db.tokens.FirstOrDefault(t => t.UserId == unprotectedUserId);
                    if (refreshToken != null) _db.tokens.Remove(refreshToken);
                   await _db.SaveChangesAsync();
                    _cookieSvc.DeleteAllCookies(cookiesToDelete);
                }
            }
            catch (Exception ex)
            {
                _cookieSvc.DeleteAllCookies(cookiesToDelete);
                Log.Error("Error while store the database logout {Error} {StackTrace} {InnerException} {Source}",
                     ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            Log.Information("Log out successfully");
            return RedirectToLocal(null);

        }

        public IActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? (IActionResult)Redirect(returnUrl):RedirectToAction(nameof(HomeController.Index),"Home");
        }



        public ActionResult AccessDenied()
        {
            return View();
        }

    }

}
