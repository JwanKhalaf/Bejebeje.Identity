using Bejebeje.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bejebeje.Identity.Data
{
  public class ApplicationDbContext : IdentityDbContext<BejebejeUser>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
  }
}
