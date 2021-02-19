using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Acupuncture.CommonFunction.ActivityFunction;
using Acupuncture.CommonFunction.CookieFunction;
using Acupuncture.Data;
using Acupuncture.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
namespace Acupuncture.CommonFunction.AuthFunction
{
    public class AuthenticateSvc:IAuthenticateSvc
    {
        //-----------------System method---------------------
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _provider;
        private readonly IDataProtector _protector;
        private readonly TokenValidationParameters _tokenValidatationParameters;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly ClaimsPrincipal _principal;
        //---------------parameters which will set in the system------
        private readonly AppSettings _appSettings;
        private readonly DataProtectionKeys _dataProtectionKeys;

        private readonly IActivitySvc _activitySvc;
        private readonly ICookieSvc _cookieSvc;

        //------------- Variant--------------
        private readonly string unProtectedToken;
        private string[] UserRoles = new[] { "Administrator", "Customer" };

        public AuthenticateSvc(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appsettingOptions,IOptions<DataProtectionKeys> dataprotectionKeyOption,
            ICookieSvc cookieSvc,IActivitySvc activitySvc,IServiceProvider provider)
        {
            _userManager = userManager;
            _dbContext = db;
            _appSettings = appsettingOptions.Value;
            _dataProtectionKeys = dataprotectionKeyOption.Value;
            _cookieSvc = cookieSvc;
            _activitySvc = activitySvc;
            _provider = provider;

        }
        public async Task<TokenResponse> Auth(LoginViewModel model) {

            try 
            {
                var user =await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return CreateErrorResponseToken("Request not supported", HttpStatusCode.Unauthorized);
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.FirstOrDefault() != "Administrator")
                {
                    Log.Error("role is not administrator");
                    return CreateErrorResponseToken("role is not admin", HttpStatusCode.Unauthorized);
                }
                //check password
                if (!await _userManager.CheckPasswordAsync(user, model.Password)) {

                    Log.Error("incorrect password");
                    return CreateErrorResponseToken("password is incorrect", HttpStatusCode.Unauthorized);
                }

                if(!await _userManager.IsEmailConfirmedAsync(user))
                {
                    Log.Error("Email is not confirmed");
                    return CreateErrorResponseToken("Email not confirmed", HttpStatusCode.Unauthorized);

                }



                var authToken =await GenerateNewToken(user, model);
                return authToken;

            }
            catch (Exception ex)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                   ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                //return await Task.FromResult(AuthenticateResult.Fail("Failed to authenticate"));
            }


            return CreateErrorResponseToken("Request not supported", HttpStatusCode.Unauthorized);
            //await Task.CompletedTask;
        }


        private async Task<TokenResponse> GenerateNewToken(ApplicationUser user, LoginViewModel model)
        {
            // Create a key to encrypt the JWT 
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            // Get user role => check if user is admin
            var roles = await _userManager.GetRolesAsync(user);

            // Creating JWT token
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName), // Sub - Identifies principal that issued the JWT
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Jti - Unique identifier of the token
                        new Claim(ClaimTypes.NameIdentifier, user.Id), // Unique Identifier of the user
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()), // Role of the user
                        new Claim("LoggedOn", DateTime.Now.ToString(CultureInfo.InvariantCulture)), // Time When Created
                 }),

                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _appSettings.Site, // Issuer - Identifies principal that issued the JWT.
                Audience = _appSettings.Audience, // Audience - Identifies the recipients that the JWT is intended for.
                Expires = (string.Equals(roles.FirstOrDefault(), "Administrator", StringComparison.CurrentCultureIgnoreCase)) ? DateTime.UtcNow.AddMinutes(60) : DateTime.UtcNow.AddMinutes(Convert.ToDouble(_appSettings.ExpireTime))
            };

            /* Create the unique encryption key for token - 2nd layer protection */
            var encryptionKeyRt = Guid.NewGuid().ToString();
            var encryptionKeyJwt = Guid.NewGuid().ToString();

            /* Get the Data protection service instance */
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();

            /* Create a protector instance */
            var protectorJwt = protectorProvider.CreateProtector(encryptionKeyJwt);

            /* Generate Token and Protect the user token */
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = protectorJwt.Protect(tokenHandler.WriteToken(token));

            /* Create and update the token table */
            Token newRtoken = new Token();

            /* Create refresh token instance */
            newRtoken = CreateRefreshToken(_appSettings.ClientId, user.Id, Convert.ToInt32(_appSettings.RtExpireTime));

            /* assign the tne JWT encryption key */
            newRtoken.EncryptionKeyJwt = encryptionKeyJwt;

            newRtoken.EncryptionKeyRt = encryptionKeyRt;

            /* Add Refresh Token with Encryption Key for JWT to DB */
            try
            {
                // First we need to check if the user has already logged in and has tokens in DB
                var rt = _dbContext.tokens
                    .FirstOrDefault(t => t.UserId == user.Id);

                if (rt != null)
                {
                    // invalidate the old refresh token (by deleting it)
                    _dbContext.tokens.Remove(rt);

                    // add the new refresh token
                    _dbContext.tokens.Add(newRtoken);

                }
                else
                {
                    await _dbContext.tokens.AddAsync(newRtoken);
                }

                // persist changes in the DB
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                        ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

            }
            // Return Response containing encrypted token
            var protectorRt = protectorProvider.CreateProtector(encryptionKeyRt);
            var layerOneProtector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);

            var encAuthToken = new TokenResponse
            {
                Token = layerOneProtector.Protect(encryptedToken),
                Expiration = token.ValidTo,
                RefreshToken = protectorRt.Protect(newRtoken.Value),
                Role = roles.FirstOrDefault(),
                Username = user.UserName,
                UserId = layerOneProtector.Protect(user.Id),
                ResponseInfo = CreateResponse("Auth Token Created", HttpStatusCode.OK)
            };

            return encAuthToken;

        }
        public static TokenResponse CreateErrorResponseToken(string errorMessage, HttpStatusCode statusCode)
        {
            var errorToken = new TokenResponse
            {
                Token = null,
                Expiration = DateTime.Now,
                RefreshTokenExpiration = DateTime.Now,
                RefreshToken = null,
                Role = null,
                Username = null,

                ResponseInfo = CreateResponse( errorMessage, statusCode)
              

        };
            return errorToken;

        }
        private static TokenResponseStatusInfo CreateResponse(string errorMessage, HttpStatusCode statusCode)
        {
            var responseInfo = new TokenResponseStatusInfo
            {
                Message = errorMessage,
                httpStatusCode = statusCode
            };
            return responseInfo;
        }
        private Token CreateRefreshToken(string clientId, string userId, int expireTime)
        {
            return new Token()
            {
                ClientId = clientId,
                UserId = userId,
                //32 digits:
                Value = Guid.NewGuid().ToString("N"),
                CreatedDate = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(expireTime),
                EncryptionKeyRt = "",
                EncryptionKeyJwt = ""
            };
        }
    }
}
