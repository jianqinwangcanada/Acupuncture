using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.ActivityFunction;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Acupuncture.CommonFunction.UserSvc
{
    public class UserSvc:IUserSvc
    {
        readonly IServiceProvider _provider;
        readonly IActivitySvc _activitySvc;
        readonly ICookieSvc _cookieSvc;
        readonly ApplicationDbContext _db;
        readonly DataProtectionKeys _dataProtectionKeys;
        readonly IHostingEnvironment _env;
        readonly UserManager<ApplicationUser> _userManager;


        public UserSvc(
          IServiceProvider provider,
        IActivitySvc activitySvc,
        ICookieSvc cookieSvc,
        ApplicationDbContext db,
        IOptions<DataProtectionKeys> dataProtectionKeys,
        IHostingEnvironment env,
         UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _provider = provider;
            _cookieSvc = cookieSvc;
            _activitySvc = activitySvc;
            _env = env;
            _db = db;
            _dataProtectionKeys = dataProtectionKeys.Value;
        }

        public async Task<ProfileModel> GetUserProfileByIdAsync(string userId)
        {
            ProfileModel profileModel = null;
            var LogedUserId = GetLoggedInUserId();
            var user = await _userManager.FindByIdAsync(LogedUserId);


            if (user == null||user.Id != userId) return null;

            try
            {
                profileModel = new ProfileModel()
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.Firstname,
                    Middlename = user.Middlename,
                    Lastname = user.Lastname,
                    IsEmailVerified = user.EmailConfirmed,
                    IsPhoneVerified = user.PhoneNumberConfirmed,
                    IsTermsAccepted = user.Terms,
                    IsTwoFactorOn = user.TwoFactorEnabled,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    IsEmployee = user.IsEmployee,
                    UseAddress = new List<Address>(await _db.addresses.Where(x => x.UserId == user.Id).Select(n =>
                       new Address()
                       {
                           AddressId = n.AddressId,
                           Line1 = n.Line1,
                           Line2 = n.Line2,
                           Unit = n.Unit,
                           Country = n.Country,
                           State = n.State,
                           City = n.City,
                           PostalCode = n.PostalCode,
                           Type = n.Type,
                           UserId = n.UserId
                       }).ToListAsync()),
                    Activities = new List<Activity>(
                         _db.activities.Where(x => x.UserId == user.Id)).OrderByDescending(d => d.Date).Take(20).ToList()


                    //Activities = new List<Activity>(
                    //    await _db.activities.Where(n=>n.UserId==user.Id).OrderByDescending(n=>n.Date).Take(20).ToListAsync())


                };
            }
            catch (Exception ex)
            {
                Log.Error("Error while Get Loggin User Profile{Error} {StackTrace} {InnerException} {Source}",
                                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return profileModel;
        }
        
        public async Task<ProfileModel> GetUserProfileByUsernameAsync(string username)
        {
            
            var userProfile = new ProfileModel();
            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);
                if (user == null || user.UserName != username) return null;

                userProfile = new ProfileModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.Firstname,
                    Middlename = user.Middlename,
                    Lastname = user.Lastname,
                    IsEmailVerified = user.EmailConfirmed,
                    IsPhoneVerified = user.PhoneNumberConfirmed,
                    IsTermsAccepted = user.Terms,
                    IsTwoFactorOn = user.TwoFactorEnabled,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    IsEmployee = user.IsEmployee,
                    UseAddress = new List<Address>(await _db.addresses.Where(x => x.UserId == user.Id).Select(n =>
                        new Address()
                        {
                            AddressId = n.AddressId,
                            Line1 = n.Line1,
                            Line2 = n.Line2,
                            Unit = n.Unit,
                            Country = n.Country,
                            State = n.State,
                            City = n.City,
                            PostalCode = n.PostalCode,
                            Type = n.Type,
                            UserId = n.UserId
                        }).ToListAsync()),
                    Activities = new List<Activity>(_db.activities.Where(x => x.UserId == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                };

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userProfile;
        }
        private string GetLoggedInUserId()
        {
            try
            {
                var protectedUid = _cookieSvc.Get("user_id");
                //If here is some problems , we should using denpendency injection
                var protectProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var unprotectedUID = protector.Unprotect(protectedUid);
                return unprotectedUID;

            }
            catch (Exception ex)
            {
                Log.Error("Error while Get Loggin User Id {Error} {StackTrace} {InnerException} {Source}",
                                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return null;

        }
        public async Task<ProfileModel> GetUserProfileByEmailAsync(string email)
        {
            var userProfile = new ProfileModel();

            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null || user.Email != email) return null;

                userProfile = new ProfileModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    Phone = user.PhoneNumber,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Displayname = user.DisplayName,
                    Firstname = user.Firstname,
                    Middlename = user.Middlename,
                    Lastname = user.Lastname,
                    IsEmailVerified = user.EmailConfirmed,
                    IsPhoneVerified = user.PhoneNumberConfirmed,
                    IsTermsAccepted = user.Terms,
                    IsTwoFactorOn = user.TwoFactorEnabled,
                    ProfilePic = user.ProfilePic,
                    UserRole = user.UserRole,
                    IsAccountLocked = user.LockoutEnabled,
                    IsEmployee = user.IsEmployee,
                    UseAddress = new List<Address>(await _db.addresses.Where(x => x.UserId == user.Id).Select(n =>
                        new Address()
                        {
                            AddressId = n.AddressId,
                            Line1 = n.Line1,
                            Line2 = n.Line2,
                            Unit = n.Unit,
                            Country = n.Country,
                            State = n.State,
                            City = n.City,
                            PostalCode = n.PostalCode,
                            Type = n.Type,
                            UserId = n.UserId
                        }).ToListAsync()),
                    Activities = new List<Activity>(_db.activities.Where(x => x.UserId == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                };

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return userProfile;
        }
        public async Task<bool> CheckPasswordAsync(ProfileModel model, string password)
        {
            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user.UserName != _cookieSvc.Get("username") ||
                    user.UserName != model.Username)
                    return false;

                if (!await _userManager.CheckPasswordAsync(user, password))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while user Check password in UserSvc  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateProfileAsync(IFormCollection formData)
        {
            var loggedInUserId = GetLoggedInUserId();
            var user = await _userManager.FindByIdAsync(loggedInUserId);

            if (user == null) return false;

            if (user.UserName != _cookieSvc.Get("username") ||
                user.UserName != formData["username"].ToString() ||
                user.Email != formData["email"].ToString())
                return false;
            
            try
            {
                Activity activityModel = new Activity { UserId = user.Id };
                await UpdateProfilePicAsync(formData, user);
                
                   
                user.Firstname = formData["firstname"];
                user.Birthday = formData["birthdate"];
                user.Lastname = formData["lastname"];
                user.Middlename = formData["middlename"];
                user.DisplayName = formData["displayname"];
                user.PhoneNumber = formData["phone"];
                user.Gender = formData["gender"];
                user.TwoFactorEnabled = Convert.ToBoolean(formData["IsTwoFactorOn"]);

                /* If Addresses exist we update them => If Addresses do not exist we add them */
                await InsertOrUpdateAddress(user.Id, "Shipping", formData["saddress1"], formData["saddress2"], formData["scountry"], formData["sstate"], formData["scity"], formData["spostalcode"], formData["sunit"]);
                await InsertOrUpdateAddress(user.Id, "Billing", formData["address1"], formData["address2"], formData["country"], formData["state"], formData["city"], formData["postalcode"], formData["unit"]);

                await _userManager.UpdateAsync(user);

                activityModel.Date = DateTime.UtcNow;
                activityModel.IpAddress = _cookieSvc.GetUserIP();
                activityModel.Location = _cookieSvc.GetUserCountry();
                activityModel.OperatingSystem = _cookieSvc.GetUserOS();
                activityModel.Type = "Profile update successful";
                activityModel.Icon = "fas fa-thumbs-up";
                activityModel.Color = "success";
                await _activitySvc.AddUserActivity(activityModel);

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while updating profile {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }
        public async Task<bool> AddUserActivity(Activity model)
        {
            try
            {
                await _activitySvc.AddUserActivity(model);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(ProfileModel model, string newPassword)
        {
            bool result;
            try
            {
                Activity activityModel = new Activity();
                activityModel.Date = DateTime.UtcNow;
                activityModel.IpAddress = _cookieSvc.GetUserIP();
                activityModel.Location = _cookieSvc.GetUserCountry();
                activityModel.OperatingSystem = _cookieSvc.GetUserOS();

                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user != null)
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
                    var updateResult = await _userManager.UpdateAsync(user);
                    result = updateResult.Succeeded;
                    activityModel.UserId = user.Id;
                    activityModel.Type = "Password Changed successful";
                    activityModel.Icon = "fas fa-thumbs-up";
                    activityModel.Color = "success";
                    await _activitySvc.AddUserActivity(activityModel);
                }
                else
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                result = false;
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return result;
        }

        public async Task<List<Activity>> GetUserActivity(string username)
        {
            List<Activity> userActivities = new List<Activity>();

            try
            {
                var loggedInUserId = GetLoggedInUserId();
                var user = await _userManager.FindByIdAsync(loggedInUserId);

                if (user == null || user.UserName != username) return null;

                userActivities = await _db.activities.Where(x => x.UserId == user.Id).OrderByDescending(o => o.Date).Take(20).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userActivities;
        }










        private async Task<ApplicationUser> UpdateProfilePicAsync(IFormCollection formData, ApplicationUser user)
        {
            // First we create an empty array to store old file info
            var oldProfilePic = new string[1];
            // we will store path of old file to delete in an empty array.
            oldProfilePic[0] = Path.Combine(_env.WebRootPath + user.ProfilePic);

            // Create the Profile Image Path
            var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";

            // If we have received any files for update, then we update the file path after saving to server
            // else we return the user without any changes
            if (formData.Files.Count <= 0) return user;

            var extension = Path.GetExtension(formData.Files[0].FileName);
            var filename = DateTime.Now.ToString("yymmssfff");
            var path = Path.Combine(profPicPath, filename) + extension;
            var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

            user.ProfilePic = dbImagePath;

            // Copying New Files to the Server - profile Folder
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await formData.Files[0].CopyToAsync(stream);
            }

            // Delete old file after successful update
            if (!System.IO.File.Exists(oldProfilePic[0])) return user;

            System.IO.File.SetAttributes(oldProfilePic[0], FileAttributes.Normal);
            System.IO.File.Delete(oldProfilePic[0]);

            return user;
        }
        private async Task InsertOrUpdateAddress(string userId, string type, string line1, string line2, string country,
            string state, string city, string postalcode, string unit)
        {
            var updateAddress = _db.addresses.FirstOrDefault(ad => ad.User.Id == userId && ad.Type == type);
            await using var dbContextTransaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var newAddress = new Address();
                if (updateAddress != null)
                {
                    updateAddress.Line1 = line1;
                    updateAddress.Line2 = line2;
                    updateAddress.Country = country;
                    updateAddress.City = city;
                    updateAddress.State = state;
                    updateAddress.PostalCode = postalcode;
                    updateAddress.Unit = unit;
                    _db.Entry(updateAddress).State = EntityState.Modified;
                }
                else
                {
                    newAddress.Line1 = line1;
                    newAddress.Line2 = line2;
                    newAddress.Country = country;
                    newAddress.City = city;
                    newAddress.State = state;
                    newAddress.PostalCode = postalcode;
                    newAddress.Unit = unit;
                    newAddress.Type = type;
                    _db.Entry(newAddress).State = EntityState.Added;
                }

                await _db.SaveChangesAsync();

                await dbContextTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbContextTransaction.RollbackAsync();

                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
    }
}
