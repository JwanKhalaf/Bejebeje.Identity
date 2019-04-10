using System.Threading.Tasks;

namespace Bejebeje.Identity.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(
            string emailAddress,
            string subject,
            string mailBody
        );
    }
}