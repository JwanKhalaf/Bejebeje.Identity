using Bejebeje.Identity.Models;

namespace Bejebeje.Identity.ViewModels
{
  public class LogoutViewModel : LogoutInputModel
  {
    public bool ShowLogoutPrompt { get; set; } = true;
  }
}
