using System.Threading.Tasks;
using Bejebeje.Identity.ViewModels;

namespace Bejebeje.Identity.Services
{
  public interface IEmailService
  {
    Task SendRegistrationEmailAsync(EmailRegistrationViewModel emailViewModel);
    Task SendForgotPasswordEmailAsync(EmailForgotPasswordViewModel emailViewModel);
  }
}