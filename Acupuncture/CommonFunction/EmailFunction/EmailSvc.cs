using System;
using System.IO;
using System.Threading.Tasks;
using Acupuncture.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Acupuncture.CommonFunction.EmailFunction
{
    public class EmailSvc:IEmailSvc
    {
       private readonly  SendGridOptions _sendGridOptions;
       private readonly  ICommonFunction _commonFunction;
       private readonly  SmtpOptions _smtpOptions;
       private readonly IWebHostEnvironment _env;

       public EmailSvc(IOptions<SendGridOptions> sendGridOptions,IOptions<SmtpOptions> smtpOptions,
           ICommonFunction commonFunction,IWebHostEnvironment env)
        {
            _env = env;
            _commonFunction = commonFunction;
            _sendGridOptions = sendGridOptions.Value;
            _smtpOptions = smtpOptions.Value;
        }
        public Task SendEmailAsync(string email, string subject, string message, string template)
        {
            var strMessageBody = BuildEmailBody(message, template, subject);

            // Check for Default emails Sending Options from App settings
            if (_sendGridOptions.IsDefault)
            {
                _commonFunction.SendEmailBySendGridAsync(_sendGridOptions.SendGridKey, _sendGridOptions.FromEmail, _sendGridOptions.FromFullName, subject, strMessageBody, email).Wait();
            }

            if (!_smtpOptions.IsDefault) return Task.CompletedTask;

            if (!string.IsNullOrEmpty(strMessageBody))
            {
                // Then we need to send email using SMTP
                _commonFunction.SendEmailByGmailAsync(_smtpOptions.FromEmail,
                    _smtpOptions.FromFullName,
                    subject,
                    strMessageBody,
                    email,
                    email,
                    _smtpOptions.SmtpUserName,
                    _smtpOptions.SmtpPassword,
                    _smtpOptions.SmtpHost,
                    _smtpOptions.SmtpPort,
                    _smtpOptions.SmtpSsl).Wait();
            }

            return Task.CompletedTask;
        }
        private string BuildEmailBody(string message, string templateName, string subject)
        {
            var strMessage = "";

            try
            {
                var strTemplateFilePath = _env.ContentRootPath + "/CommonFunction/EmailTemplates/" + templateName;
                var reader = new StreamReader(strTemplateFilePath);
                strMessage = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            strMessage = strMessage.Replace("[[[Title]]]", string.IsNullOrEmpty(subject) ? "Notification => Yiyang Acupuncuture" : subject);
            strMessage = strMessage.Replace("[[[message]]]", message);
            return strMessage;
        }


    }
}
