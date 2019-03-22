using Bejebeje.Identity.Configuration;
using Bejebeje.Identity.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;

namespace Bejebeje.Identity.Data
{
  public class DataSeeder
  {
    private InitialSeedConfiguration seedConfiguration { get; set; }

    public DataSeeder(IOptions<InitialSeedConfiguration> initialSeedConfiguration)
    {
      seedConfiguration = initialSeedConfiguration.Value;
    }

    public void EnsureDataIsSeeded()
    {
      ServiceCollection services = new ServiceCollection();

      services
        .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(seedConfiguration.ConnectionString));

      services
        .AddIdentity<BejebejeUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      using (ServiceProvider serviceProvider = services.BuildServiceProvider())
      {
        using (IServiceScope scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
          ApplicationDbContext context = scope
            .ServiceProvider
            .GetService<ApplicationDbContext>();

          context.Database.Migrate();

          UserManager<BejebejeUser> userManager = scope
            .ServiceProvider
            .GetRequiredService<UserManager<BejebejeUser>>();

          BejebejeUser seedUser = userManager
            .FindByNameAsync(seedConfiguration.Username)
            .Result;

          if (seedUser == null)
          {
            seedUser = new BejebejeUser
            {
              UserName = seedConfiguration.Username,
              Email = seedConfiguration.Email,
              EmailConfirmed = true,
              DisplayUsername = seedConfiguration.FirstName
            };

            IdentityResult result = userManager
              .CreateAsync(seedUser, seedConfiguration.Password)
              .Result;

            if (!result.Succeeded)
            {
              throw new Exception(result.Errors.First().Description);
            }

            result = userManager
              .AddClaimsAsync(
                seedUser,
                new Claim[] {
                  new Claim(JwtClaimTypes.Name, $"{seedConfiguration.FirstName} {seedConfiguration.LastName}"),
                  new Claim(JwtClaimTypes.GivenName, seedConfiguration.FirstName),
                  new Claim(JwtClaimTypes.FamilyName, seedConfiguration.LastName),
                  new Claim(JwtClaimTypes.Email, seedConfiguration.Email),
                  new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                  new Claim(JwtClaimTypes.WebSite, seedConfiguration.Website)
                })
              .Result;

            if (!result.Succeeded)
            {
              throw new Exception(result.Errors.First().Description);
            }

            Console.WriteLine($"{seedConfiguration.Username} created");
          }
          else
          {
            Console.WriteLine($"{seedConfiguration.Username} already exists");
          }
        }
      }
    }
  }
}
