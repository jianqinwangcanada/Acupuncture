using System;
using System.Threading.Tasks;
using Acupuncture.Model;

namespace Acupuncture.CommonFunction.AuthFunction
{
    public interface IAuthenticateSvc
    {
        Task<TokenResponse> Auth(LoginViewModel model);
    }
}
