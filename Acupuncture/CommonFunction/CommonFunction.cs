using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Acupuncture.CommonFunction
{
    public class ComFunction
    {
        private readonly AdminUserOptions _adminUserOptions;
        private readonly AppUserOptions _appUserOptions;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IHostingEnvironment _hostEnv;

        
        //dependency injection to pupulate the object
        public ComFunction(IOptions<AdminUserOptions> adminOptions,
            IOptions<AppUserOptions >appOptions,
            UserManager<ApplicationUser> userManager,IHostingEnvironment env)
        {
            _adminUserOptions = adminOptions.Value;
            _appUserOptions = appOptions.Value;
            _UserManager = userManager;
            _hostEnv = env;
        }
        public async Task CreateAdminUser()
        {
            try
            {
                var adminUser = new ApplicationUser
                {
                    Email = _adminUserOptions.Email,
                    UserName = _adminUserOptions.Username,
                    EmailConfirmed = true,
                    ProfilePic = "",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Firstname = _adminUserOptions.Firstname,
                    Lastname = _adminUserOptions.Lastname,
                    UserRole = "Administrator",
                    IsActive = true,
                    UserAddresses = new List<Address>
                {
                    new Address{Country=_adminUserOptions.Country,Type="shipping"},
                    new Address{Country=_adminUserOptions.Country,Type="billing"}
                }
                };
                var result = await _UserManager.CreateAsync(adminUser, _adminUserOptions.Password);
                if (result.Succeeded)
                {
                     await _UserManager.AddToRoleAsync(adminUser, "Administrator");
                    Log.Information("Admin User Created {UserName}", adminUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            


        }

        public async Task CreateAppUser()
        {
            try
            {
                var appUser = new ApplicationUser
                {
                    Email = _appUserOptions.Email,
                    UserName = _appUserOptions.Username,
                    EmailConfirmed = true,
                    ProfilePic = "",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Firstname = _appUserOptions.Firstname,
                    Lastname = _appUserOptions.Lastname,
                    UserRole = "Customer",
                    IsActive = true,
                    UserAddresses = new List<Address>
                    {
                        new Address {Country = _appUserOptions.Country, Type = "Billing"},
                        new Address {Country = _appUserOptions.Country, Type = "Shipping"}
                    }
                };

                var result = await _UserManager.CreateAsync(appUser, _appUserOptions.Password);

                if (result.Succeeded)
                {
                    await _UserManager.AddToRoleAsync(appUser, "Customer");
                    Log.Information("App User Created {UserName}", appUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
    }
}
