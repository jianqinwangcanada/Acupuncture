using System;
using System.Text;
using Acupuncture.CommonFunction;
using Acupuncture.CommonFunction.ActivityFunction;
using Acupuncture.CommonFunction.AuthFunction;
using Acupuncture.CommonFunction.BackEndFunction;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.CommonFunction.CountryFunction;
using Acupuncture.CommonFunction.EmailFunction;
using Acupuncture.CommonFunction.Extensions;
using Acupuncture.CommonFunction.RoleSvc;
using Acupuncture.CommonFunction.UserSvc;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Acupuncture
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //-------------------DbContext Injection-------------
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseMySQL(Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("Acupuncture")));
            services.AddDbContext<DataProtectionContext>(options=>
            options.UseMySQL(Configuration.GetConnectionString("DataProtectionKeysContext"),x=>x.MigrationsAssembly("Acupuncture")));

            //=========================Register CommonManagFunction=======================
            services.AddTransient<ICommonFunction, ComFunction>();

            //----------------------------------------------------------------------
            //                  Populate the the the default users from setting-
            //----------------------------------------------------------------------
            services.Configure<AdminUserOptions>(Configuration.GetSection("AdminUserOptions"));
            services.Configure<AppUserOptions>(Configuration.GetSection("AppUserOptions"));

            //----------------------------------------------------------------------
            //                        Default Identity
            //----------------------------------------------------------------------
           
            var identityOptionsConfiguration = Configuration.GetSection("IdentityDefaultOptions");
            services.Configure<IdentityDefaultOptions>(identityOptionsConfiguration);
            var identityDefaultOptions = identityOptionsConfiguration.Get<IdentityDefaultOptions>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = identityDefaultOptions.PasswordRequireDigit;
                options.Password.RequiredLength = identityDefaultOptions.PasswordRequiredLength;
                options.Password.RequireNonAlphanumeric = identityDefaultOptions.PasswordRequireNonAlphanumeric;
                options.Password.RequireUppercase = identityDefaultOptions.PasswordRequireUppercase;
                options.Password.RequireLowercase = identityDefaultOptions.PasswordRequireLowercase;
                options.Password.RequiredUniqueChars = identityDefaultOptions.PasswordRequiredUniqueChars;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
                options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = identityDefaultOptions.LockoutAllowedForNewUsers;

                // User settings
                options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

                // email confirmation require
                options.SignIn.RequireConfirmedEmail = identityDefaultOptions.SignInRequireConfirmedEmail;

            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services.AddMvc().AddControllersAsServices().AddRazorRuntimeCompilation().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //=================================================================================
            //                        DataProtection
            //==================================================================================

            var dataprotectionSection = Configuration.GetSection("DataProtectionKeys");
            services.Configure<DataProtectionKeys>(dataprotectionSection);
            services.AddDataProtection().PersistKeysToDbContext<DataProtectionContext>();


            //=======================================Appseeting Options===========================
            var appSettingSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingSection);

            //=================================================================================
            //                        Jwt Authentication
            //==================================================================================
            var appSettings = appSettingSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(o => {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = appSettings.ValidateIssuerSigningKey,
                    ValidateIssuer = appSettings.ValidateIssuer,
                    ValidateAudience = appSettings.ValidateAudience,
                    ValidIssuer = appSettings.Site,
                    ValidAudience = appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero

                };
            });

            //+++++++++++++++++++++++++++++++Email Service++++++++++++++++++++++++++++++++++++++++
            services.Configure<SmtpOptions>(Configuration.GetSection("SmtpOptions"));
            services.Configure<SendGridOptions>(Configuration.GetSection("SendGridOptions"));

            services.AddTransient<IEmailSvc, EmailSvc>();

            //---------------------------------------------------------------------------------

            //+++++++++++++++++++++++++++++++Country Service++++++++++++++++++++++++++++++++++++++++
            services.AddTransient<IUserSvc, UserSvc>();

            //---------------------------------------------------------------------------------
            //                           Role Service
            //----------------------------------------------------------------------------------
            services.AddTransient<IRoleSvc, RoleSvc>();

            //+++++++++++++++++++++++++++++++Country Service++++++++++++++++++++++++++++++++++++++++
            services.AddTransient<ICountrySvc, CountrySvc>();

            //+++++++++++++++++++++++++++++++Activity Service++++++++++++++++++++++++++++++++++++++++
            services.AddTransient<IActivitySvc, ActivitySvc>();

            //---------------------------------------------------------------------------------
            //                  Auth Service
            //----------------------------------------------------------------------------------
            services.AddTransient<IAuthenticateSvc, AuthenticateSvc>();

            //---------------------------------------------------------------------------------
            //                 Admin   Service
            //----------------------------------------------------------------------------------
            services.AddTransient<IAdminSvc, AdminSvc>();

            //---------------------------------------------------------------------------------
            //                  Add Writeble Appsetting Service
            //----------------------------------------------------------------------------------
             var writebleSettingConfigSection= Configuration.GetSection("SiteWideSettings");
            // services.Configure<SiteWideSettings>(writebleSettingConfigSection);
            var sendGridOptionsSection = Configuration.GetSection("SendGridOptions");
            var smtpOptionsSection = Configuration.GetSection("SmtpOptions");
            services.ConfigWritableSetting<SendGridOptions>(sendGridOptionsSection, "appsettings.json");
            services.ConfigWritableSetting<SmtpOptions>(smtpOptionsSection, "appsettings.json");
            services.ConfigWritableSetting<SiteWideSettings>(writebleSettingConfigSection, "appsettings.json");



            //------------------------------------------------------------ ---------------------
            //                                  Add cookie fucntion service 
            //------------------------------------------------------------------ ---------------------
            services.AddHttpContextAccessor();
            services.AddTransient<CookieOptions>();
            services.AddTransient<ICookieSvc, CookieSvc>();

            //-------------------------------------------------------------------------------
            //                   Authentication Service
            //------------------------------------------------------------------------------------
            services.AddAuthentication("Administrator").AddScheme<AdminAuthenticationOptions, AdminAuthenticationHandler>("Admin", null);
            services.AddCors(options=> {
                options.AddPolicy("EnableCORS",buider=> {
                    buider.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
                });

            });

            //++++++++++++++++-----------------------------------------------------------------------
            //                   Enable APi Version
            //------------------------------------------------------------------------------------

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);


            });




            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("EnableCORS");
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
           
            app.UseEndpoints(endpoints =>
            {
            endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
