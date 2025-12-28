using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostCare.core.Interfaces
{

    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendOtpEmailAsync(string toEmail, string otpCode, string userName);
        Task SendPasswordResetOtpEmailAsync(string toEmail, string otpCode, string userName);
    }
}
