using Microsoft.AspNetCore.Identity;

namespace Bejebeje.Identity.Models
{
  public class BejebejeUser : IdentityUser
  {
    public string DisplayUsername { get; set; }
  }
}
