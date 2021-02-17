using System;
using System.Security.Claims;

namespace Acupuncture.Model
{
    public class TokenResponse
    {
        public string Token { get; set; } // jwt token
        public DateTime Expiration { get; set; } // expiry time
        public DateTime RefreshTokenExpiration { get; set; } // expiry time
        public string RefreshToken { get; set; } // refresh token
        public string Role { get; set; } // user role
        public string Username { get; set; } // user name
        public string UserId { get; set; } // user id
        public bool TwoFactorLoginOn { get; set; } // if two factor validation is on
        public ClaimsPrincipal Principal { get; set; }
        public TokenResponseStatusInfo ResponseInfo { get; set; } // status
    }
}
