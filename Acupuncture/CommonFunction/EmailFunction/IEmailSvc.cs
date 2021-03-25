using System;
using System.Threading.Tasks;

namespace Acupuncture.CommonFunction.EmailFunction
{
    public interface IEmailSvc
    {
        Task SendEmailAsync(string email, string subject, string message, string template);
    }
}
