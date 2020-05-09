namespace Bejebeje.Identity.Services
{
  using System.Threading.Tasks;
  using ViewModels;

  public interface IEmailService
  {
    Task SendRegistrationEmailAsync(EmailRegistrationViewModel emailViewModel);

    Task SendForgotPasswordEmailAsync(EmailForgotPasswordViewModel emailViewModel);
  }
}