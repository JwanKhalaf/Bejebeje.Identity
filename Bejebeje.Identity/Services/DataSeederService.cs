namespace Bejebeje.Identity.Services
{
  using Bejebeje.Identity.Configuration;
  using Bejebeje.Identity.Data;
  using Bejebeje.Identity.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using System;
  using System.Threading.Tasks;

  public class DataSeederService : IDataSeederService
  {
    private InitialSeedConfiguration seedConfiguration { get; set; }

    private ApplicationDbContext context;

    private UserManager<BejebejeUser> userManager;

    private ILogger<DataSeederService> logger;

    public DataSeederService(
      IOptions<InitialSeedConfiguration> initialSeedConfiguration,
      ApplicationDbContext context,
      UserManager<BejebejeUser> userManager,
      ILogger<DataSeederService> logger
      )
    {
      this.seedConfiguration = initialSeedConfiguration.Value;
      this.context = context;
      this.userManager = userManager;
      this.logger = logger;
    }

    public async Task SeedDataAsync()
    {
      await context.Database.MigrateAsync();

      BejebejeUser seedUser = await userManager
            .FindByNameAsync(seedConfiguration.Username);

      if (seedUser == null)
      {
        seedUser = new BejebejeUser
        {
          UserName = seedConfiguration.Username,
          Email = seedConfiguration.Email,
          EmailConfirmed = true,
          DisplayUsername = seedConfiguration.FirstName
        };

        IdentityResult identityResult = await userManager
          .CreateAsync(seedUser, seedConfiguration.Password);

        if (!identityResult.Succeeded)
        {
          throw new Exception(identityResult.ToString());
        }

        logger.LogInformation($"{seedConfiguration.Username} created");
      }
      else
      {
        logger.LogError($"{seedConfiguration.Username} already exists");
      }
    }
  }
}
