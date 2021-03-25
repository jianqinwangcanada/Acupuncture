using System;
using System.Threading.Tasks;

namespace Acupuncture.CommonFunction
{
    public interface ICommonFunction
    {
         Task CreateAdminUser();
         Task CreateAppUser();
        Task SendEmailByGmailAsync(string fromEmail, string fromFullName, string subject, string messageBody, string toEmail, string toFullName, string smtpUser, string smtpPassword, string smtpHost, int smtpPort, bool smtpSSL);
        Task SendEmailBySendGridAsync(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email);
    }
}
