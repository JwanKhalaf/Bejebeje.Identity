using Bejebeje.Identity.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Linq;

namespace Bejebeje.Identity
{
  public class Program
  {
    public static void Main(string[] args)
    {
      bool seedIsRequested = args.Any(x => x == "/seed");

      if (seedIsRequested)
      {
        args = args
          .Except(new string[] { "/seed" })
          .ToArray();
      }

      var host = CreateWebHostBuilder(args).Build();

      if (seedIsRequested)
      {
        var config = host.Services.GetRequiredService<IConfiguration>();
        var connectionString = config.GetConnectionString("DefaultConnection");
        SeedData.EnsureDataIsSeeded(connectionString);
        return;
      }

      host.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
      return WebHost
        .CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .UseSerilog(
          (context, configuration) =>
          {
            configuration
              .MinimumLevel.Debug()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .MinimumLevel.Override("System", LogEventLevel.Warning)
              .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
              .Enrich.FromLogContext()
              .WriteTo.File(@"identityserver4_log.txt")
              .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                theme: AnsiConsoleTheme.Literate);
          });
    }
  }
}
