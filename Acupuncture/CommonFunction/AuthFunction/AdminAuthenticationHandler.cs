using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Acupuncture.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace Acupuncture.CommonFunction.AuthFunction
{
    public class AdminAuthenticationHandler : AuthenticationHandler<AdminAuthenticationOptions>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _provider;
        private readonly IdentityDefaultOptions _identityDefaultOptions;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;
        private const string Access_Totken = "access_token";
        private const string User_Id = "user_id";
        private const string UserName = "username";
        private string[] UserRoles = new[] { "Administrator" };

        public AdminAuthenticationHandler(IOptionsMonitor<AdminAuthenticationOptions> options,ILoggerFactory logger,UrlEncoder encoder,ISystemClock clock,
          UserManager<ApplicationUser>  userManager,
         IServiceProvider provider,
         IOptions<IdentityDefaultOptions> identityDefaultOptions,
         IOptions<DataProtectionKeys> dataProtectionKeys,
         IOptions<AppSettings> appSettings
            ):base(options,logger,encoder,clock)
        {
            _userManager = userManager;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            _provider = provider;
            _identityDefaultOptions = identityDefaultOptions.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //(1)Check whether Request contains tokens
            if (!Request.Cookies.ContainsKey(Access_Totken) || !Request.Cookies.ContainsKey(User_Id))
            {
                Log.Error("token doesn't find");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[Access_Totken]}",out AuthenticationHeaderValue headerTokenValue))
            {
                Log.Error("Couldn't parse token from cookies");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[User_Id]}",out AuthenticationHeaderValue headerValueUid))
            {
                Log.Error("Couldn't parse User Id from cookies");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            try
            {
                //get the token used secret
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                //jwt token handler on second layer
                var handler = new JwtSecurityTokenHandler();
                //preparing the validation parameters for jwthandler  on second layer
                TokenValidationParameters tokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = _appSettings.Site,
                        ValidAudience = _appSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero

                    };
                /*  Get the Data protection service instance */
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();

                /*  create a protector instance */
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);

                /* STEP 6. Layer One Unprotect the user id */
                var decryptedUid = protector.Unprotect(headerValueUid.Parameter);

                /* STEP 7. Layer One Unprotect the user token */
                var decryptedToken = protector.Unprotect(headerTokenValue.Parameter);
                Token token = new Token();
                using (var scope = _provider.CreateScope()) {

                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var userToken = dbContext.tokens.Include(x => x.User).FirstOrDefault(
                        utoken=>utoken.UserId==decryptedUid&&
                        utoken.User.UserName==Request.Cookies[UserName]&&
                        utoken.User.Id==decryptedUid &&
                        utoken.User.UserRole == "Administrator" );
                       token = userToken;
                }
                if (token == null) {
                    return await Task.FromResult(AuthenticateResult.Fail("There is no token from database equal from brower"));
                }

                /* STEP 12. Apply second layer of decryption using the key store in the token model */
                /* STEP 12.1 Create Protector instance for layer two using token model key */
                /* IMPORTANT - If no key exists or key is invalid - exception will be thrown */
                IDataProtector layerTwoProtector = protectorProvider.CreateProtector(token?.EncryptionKeyJwt);
                string decryptedTokenLayerTwo = layerTwoProtector.Unprotect(decryptedToken);


                /* STEP 13. Validate the token we received - using validation parameters set in step 3 */
                /* IMPORTANT - If the validation fails - the method ValidateToken will throw exception */
                var validateToken = handler.ValidateToken(decryptedTokenLayerTwo, tokenValidationParameters, out var securityToken);

                /* STEP 14. Checking Token Signature */
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Your are not authorized, Failed at layer two"));
                }

                /* STEP 15. Extract the username from the validated token */
                var username = validateToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                if (Request.Cookies[UserName] != username)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));
                }

                var user = await _userManager.FindByNameAsync(username);
                if (user == null) {
                    return await Task.FromResult(AuthenticateResult.Fail("Cann't get user using username on the second layer"));
                }
                if (!UserRoles.Contains(user.UserRole)) {

                    return await Task.FromResult(AuthenticateResult.Fail("Faild user role"));
                }
                var identity = new ClaimsIdentity(validateToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return await Task.FromResult(AuthenticateResult.Success(ticket));

            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return await Task.FromResult(AuthenticateResult.Fail("Failed to authenticate"));
            }
        }



        /// Override this method to handle Forbid.
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Cookies.Delete(Access_Totken);
            Response.Cookies.Delete(User_Id);
            Response.Headers["WWW-Authenticate"] = $"Not Authorized";
            Response.Redirect(_identityDefaultOptions.AccessDeniedPath);
            return Task.CompletedTask;
        }
    }
}
