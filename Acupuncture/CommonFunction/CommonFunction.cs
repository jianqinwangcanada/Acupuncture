using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;

namespace Acupuncture.CommonFunction
{
    public class ComFunction:ICommonFunction
    {
        private readonly AdminUserOptions _adminUserOptions;
        private readonly AppUserOptions _appUserOptions;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly IWebHostEnvironment _env;
        // private readonly IHostingEnvironment _hostEnv;


        //dependency injection to pupulate the object

        public ComFunction(IOptions<AdminUserOptions> adminOptions,
            IOptions<AppUserOptions >appOptions,
            UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _adminUserOptions = adminOptions.Value;
            _appUserOptions = appOptions.Value;
            _UserManager = userManager;
           _env = env;
        }
        public async Task CreateAdminUser()
        {
            Console.WriteLine("----Call the create user method---fuck");
            try
            {
                var adminUser = new ApplicationUser
                {
                    Email = _adminUserOptions.Email,
                    UserName = _adminUserOptions.Username,
                    EmailConfirmed = true,
                    ProfilePic = "/uploads/user/profile/default/profile.jpeg",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Firstname = _adminUserOptions.Firstname,
                    Lastname = _adminUserOptions.Lastname,
                    UserRole = "Administrator",
                    IsActive = true,                    
                    UserAddresses = new List<Address>
                {
                    new Address{Country=_adminUserOptions.Country,Type="Shipping"},
                    new Address{Country=_adminUserOptions.Country,Type="Billing"}
                }

                
                };
                Console.WriteLine("----adminUser-----------");
                Console.WriteLine(adminUser.Firstname + ":" + adminUser.UserAddresses.ToString());
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
                    ProfilePic = await GetDefaultProfilePic(),
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
                Console.WriteLine("----adminUser-----------");
                Console.WriteLine(appUser.Firstname + ":" + appUser.UserAddresses.ToString());
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
        private async Task<string> GetDefaultProfilePic()
        {
            try
            {
                // Default Profile pic path
                // Create the Profile Image Path
                var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";
                var defaultPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}default{Path.DirectorySeparatorChar}profile.jpeg";
                var extension = Path.GetExtension(defaultPicPath);
                var filename = DateTime.Now.ToString("yymmssfff");
                var path = Path.Combine(profPicPath, filename) + extension;
                var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

                await using (Stream source = new FileStream(defaultPicPath, FileMode.Open))
                {
                    await using Stream destination = new FileStream(path, FileMode.Create);
                    await source.CopyToAsync(destination);
                }

                return dbImagePath;

            }
            catch (Exception ex)
            {
                Log.Error("{Error}", ex.Message);
            }

            return string.Empty;
        }
    }
}
