namespace Bejebeje.Identity.Models
{
  using Microsoft.AspNetCore.Identity;

  public class BejebejeUser : IdentityUser
  {
    public string DisplayUsername { get; set; }
  }
}
