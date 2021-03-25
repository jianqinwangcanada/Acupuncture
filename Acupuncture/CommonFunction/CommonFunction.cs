using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;
using Ubiety.Dns.Core;
using SendGrid;
using SendGrid.Helpers.Mail;

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


        public async Task SendEmailByGmailAsync(string fromEmail, string fromFullName, string subject, string messageBody, string toEmail, string toFullName, string smtpUser, string smtpPassword, string smtpHost, int smtpPort, bool smtpSSL)
        {
            try
            {
                var body = messageBody;
                var message = new MailMessage();
                message.To.Add(new MailAddress(toEmail, toFullName));
                message.From = new MailAddress(fromEmail, fromFullName);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtp = new SmtpClient();
                var credential = new NetworkCredential
                {
                    UserName = smtpUser,
                    Password = smtpPassword
                };
                smtp.Credentials = credential;
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = smtpSSL;
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while sending email {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

        }

        public async Task SendEmailBySendGridAsync(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email)
        {
            try
            {
                await Execute(apiKey, fromEmail, fromFullName, subject, message, email);
            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        static async Task<SendGrid.Response> Execute(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromFullName);
            var to = new EmailAddress(email);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }


    }
}

