using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.CommonFunction.UserSvc;
using Acupuncture.CommonFunction.WritebleAppSettingFunction;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Acupuncture.CommonFunction.AttributeFuction;
using Acupuncture.CommonFunction.RoleSvc;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acupuncture.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    [Authorize(AuthenticationSchemes = "Admin")]
    public class UserRoleController : Controller
    {
        private readonly IUserSvc _userSvc;
        private readonly IRoleSvc _roleSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;
        private AdminBaseViewModel _adminBaseViewModel;
        private readonly IWritebleSettingSvc<SiteWideSettings> _writableSiteWideSettings;
        public UserRoleController(
            IUserSvc userSvc, ICookieSvc cookieSvc, IServiceProvider provider,
            IOptions<DataProtectionKeys> dataProtectionKeys, IOptions<AppSettings> appSettings,
            IWritebleSettingSvc<SiteWideSettings> writableSiteWideSettings,IRoleSvc roleSvc)
        {
            _userSvc = userSvc;
            _roleSvc = roleSvc;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            _writableSiteWideSettings = writableSiteWideSettings;
            
            
        }

        public async Task<IActionResult> Index()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIdAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            _adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSetting = _appSettings,
                SiteWideSetting = _writableSiteWideSettings.Value,
                PermissionTypes = (List<PermissionType>)await _roleSvc.GetAllRolePermissionsTypesAsync()

            };
            return View("Index", _adminBaseViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Permissions()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIdAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            _adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSetting = _appSettings,
                SiteWideSetting = _writableSiteWideSettings.Value,
                PermissionTypes = (List<PermissionType>)await _roleSvc.GetAllRolePermissionsTypesAsync()
            };
           // (List<PermissionType>)await _roleSvc.GetAllRolePermissionsTypesAsync();


            return View("Permissions",_adminBaseViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> AddPermissionType(string permissionTypeName="")
        {
            Console.WriteLine("fuck");
            if (permissionTypeName == null || permissionTypeName == "") {
                return BadRequest();
            }

            
            var result = await _roleSvc.AddRolePermissionAsync(permissionTypeName);

            return RedirectToAction("Permissions"); 
           // return View("Index", _adminBaseViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> DeletePermissionType( int permissionId = 0)
        {
           
            Console.WriteLine(permissionId);
            var result = await _roleSvc.DeleteRolePermissionTypesAsync(permissionId);

            return RedirectToAction("Permissions");
            // return View("Index", _adminBaseViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> EditPermissionType( int permissionTypeId = 0,string PermissionNewType = "")
        {
            Console.WriteLine("fuckedit");
            Console.WriteLine(permissionTypeId);
            
            Console.WriteLine(PermissionNewType);
            var result = await _roleSvc.UpdatePermissionTypeAsync(new PermissionType {Id= permissionTypeId, Type=PermissionNewType });

            return RedirectToAction("Permissions");
            // return View("Index", _adminBaseViewModel);
        }






        [AjaxAccessOnly]
        public IActionResult GetRoles(string routeId)
        {
            return PartialView("_GetRolesLayout");
        }
    }
}
