using Microsoft.Extensions.Configuration;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");

            using var smtpClient = new SmtpClient(smtpSettings["SmtpServer"])
            {
                Port = int.Parse(smtpSettings["Port"]),
                Credentials = new NetworkCredential(
                    smtpSettings["Username"],
                    smtpSettings["Password"]
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Email Verification - PostCare";
            var body = $@"
                <html>

<body style='font-family: BrandFont;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #ff3381;'>Welcome to PostCare,
            {userName}

            ! 👶</h2>
        <p>Please verify your email address using the code below:</p>
        <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
            <h1 style='color: #ff3381; letter-spacing: 10px; margin: 0;'> {otpCode}

            </h1>
        </div>
        <p>This code will expire in <strong>1 minutes</strong>.</p>
        <p>If you didn't create an account, please ignore this email.</p>
        <hr style='margin-top: 30px;'>
        <p style='color: #666; font-size: 12px;'>PostCare - Your Motherhood Journey Companion</p>
    </div>
</body>

</html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
        public async Task SendPasswordResetOtpEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Password Reset OTP - PostCare";
            var body = $@"
            <html>
            <body style='font-family: BrandFont;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #ff3381;'>Password Reset Request</h2>
                    <p>Hello {userName},</p>
                    <p>We received a request to reset your password. Use the code below to reset it:</p>
                    
                    <div style='background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #ff3381; letter-spacing: 10px; margin: 0;'>{otpCode}</h1>
                    </div>
                    
                    <p>This code will expire in <strong>10 minutes</strong>.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    
                    <hr style='margin-top: 30px;'>
                    <p style='color: #666; font-size: 12px;'>PostCare - Your Motherhood Journey Companion</p>
                </div>
            </body>
            </html>
        ";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}