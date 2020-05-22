namespace Bejebeje.Identity.Services
{
  using System.Collections.Generic;
  using System.Security.Claims;
  using IdentityServer4.Models;
  using IdentityServer4.Services;
  using System.Threading.Tasks;
  using IdentityModel;
  using Microsoft.AspNetCore.Identity;
  using Models;

  public class ProfileService : IProfileService
  {
    private readonly UserManager<BejebejeUser> _userManager;

    public ProfileService(UserManager<BejebejeUser> userManager)
    {
      _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
      BejebejeUser user = await _userManager.GetUserAsync(context.Subject);

      IList<string> roles = await _userManager.GetRolesAsync(user);

      IList<Claim> roleClaims = new List<Claim>();

      foreach (string role in roles)
      {
        roleClaims.Add(new Claim(JwtClaimTypes.Role, role));
      }

      context.IssuedClaims.AddRange(roleClaims);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
      return Task.CompletedTask;
    }
  }
}
