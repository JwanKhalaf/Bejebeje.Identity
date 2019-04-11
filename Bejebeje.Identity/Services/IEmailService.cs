using System.Threading.Tasks;

namespace Bejebeje.Identity.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string emailAddress,
            string emailSubject,
            string emailBody
        );
    }
}