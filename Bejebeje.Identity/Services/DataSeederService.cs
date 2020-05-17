namespace Bejebeje.Identity.Services
{
  using Configuration;
  using Data;
  using Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using System;
  using System.Threading.Tasks;

  public class DataSeederService : IDataSeederService
  {
    private InitialSeedConfiguration SeedConfiguration { get; }

    private readonly ApplicationDbContext _context;

    private readonly UserManager<BejebejeUser> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly ILogger<DataSeederService> _logger;

    public DataSeederService(
      IOptions<InitialSeedConfiguration> initialSeedConfiguration,
      ApplicationDbContext context,
      UserManager<BejebejeUser> userManager,
      RoleManager<IdentityRole> roleManager,
      ILogger<DataSeederService> logger
      )
    {
      SeedConfiguration = initialSeedConfiguration.Value;
      _context = context;
      _userManager = userManager;
      _roleManager = roleManager;
      _logger = logger;
    }

    public async Task SeedDataAsync()
    {
      Console.WriteLine("Seeding the database.");

      await _context.Database.MigrateAsync();

      IdentityRole adminRole = await _roleManager.FindByNameAsync("administrator");

      if (adminRole == null)
      {
        adminRole = new IdentityRole("administrator");

        IdentityResult identityResult = await _roleManager.CreateAsync(adminRole);

        if (!identityResult.Succeeded)
        {
          throw new Exception(identityResult.ToString());
        }

        _logger.LogInformation("created the administrator role.");
      }

      IdentityRole moderatorRole = await _roleManager.FindByNameAsync("moderator");

      if (moderatorRole == null)
      {
        moderatorRole = new IdentityRole("moderator");

        IdentityResult identityResult = await _roleManager.CreateAsync(moderatorRole);

        if (!identityResult.Succeeded)
        {
          throw new Exception(identityResult.ToString());
        }

        _logger.LogInformation("created the moderator role.");
      }

      BejebejeUser seedUser = await _userManager
            .FindByNameAsync(SeedConfiguration.Username);

      if (seedUser == null)
      {
        seedUser = new BejebejeUser
        {
          UserName = SeedConfiguration.Username,
          Email = SeedConfiguration.Email,
          EmailConfirmed = true,
          DisplayUsername = SeedConfiguration.FirstName
        };

        IdentityResult identityResult = await _userManager
          .CreateAsync(seedUser, SeedConfiguration.Password);

        if (!identityResult.Succeeded)
        {
          throw new Exception(identityResult.ToString());
        }

        _logger.LogInformation($"{SeedConfiguration.Username} created");

        IdentityResult identityResultOnRoleAssignment = await _userManager
          .AddToRoleAsync(seedUser, "administrator");

        if (!identityResultOnRoleAssignment.Succeeded)
        {
          throw new Exception(identityResultOnRoleAssignment.ToString());
        }

        _logger.LogInformation("assigned seed user to administrator role.");
      }
      else
      {
        _logger.LogError($"{SeedConfiguration.Username} already exists");
      }
    }
  }
}
