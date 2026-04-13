using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace cuahanggiay.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var smtpServer = _configuration["EmailConfiguration:SmtpServer"] ?? "smtp.gmail.com";
                var portString = _configuration["EmailConfiguration:Port"] ?? "587";
                int port = int.Parse(portString);
                var senderName = _configuration["EmailConfiguration:SenderName"] ?? "Vua Giày Hiệu";
                var username = _configuration["EmailConfiguration:Username"] ?? "";
                var password = (_configuration["EmailConfiguration:Password"] ?? "").Replace(" ", "");

                if (string.IsNullOrEmpty(username) || username == "your-email@gmail.com" || string.IsNullOrEmpty(password) || password == "your-app-password")
                {
                    _logger.LogWarning("Chưa cấu hình Email/Password thực tế trong appsettings.json. Bỏ qua gửi email thực tế để tránh lỗi.");
                    return;
                }

                using var client = new SmtpClient(smtpServer, port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, senderName),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true,
                };
                
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Không thể gửi email đến {toEmail}");
            }
        }
    }
}
