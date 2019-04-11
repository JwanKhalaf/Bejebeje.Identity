using System.Threading.Tasks;

namespace Bejebeje.Identity.Services
{
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(
            string emailAddress,
            string emailSubject,
            string emailBody
        )
        {
            throw new System.Exception("Not implemented.");
        }
    }
}