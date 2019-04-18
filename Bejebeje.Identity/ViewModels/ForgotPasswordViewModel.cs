using System.ComponentModel.DataAnnotations;

namespace Bejebeje.Identity.ViewModels
{
  public class ForgotPasswordViewModel
  {
    [Required]
    [EmailAddress]
    public string Email { get; set; }
  }
}
