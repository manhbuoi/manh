using System.Threading.Tasks;

namespace cuahanggiay.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}
