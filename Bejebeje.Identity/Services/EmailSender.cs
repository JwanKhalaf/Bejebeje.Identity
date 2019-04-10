using System.Threading.Tasks;

namespace Bejebeje.Identity.Services
{
    public class EmailSender : IEmailSender
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