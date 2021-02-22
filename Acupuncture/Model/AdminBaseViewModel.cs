using System;
namespace Acupuncture.Model
{
    public class AdminBaseViewModel
    {
        public ProfileModel Profile { get; set; }
        public AddUserModel AddUser { get; set; }
        public DashBoardModel Dashboard { get; set; }
        public AppSettings AppSetting { get; set; }
        public SendGridOptions SendGridOption { get; set; }
        public SmtpOptions SmtpOption { get; set; }
        public ResetPasswordViewModel ResetPassword { get; set; }
        public SiteWideSettings SiteWideSetting { get; set; }
    }
}
