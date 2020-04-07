namespace Bejebeje.Identity
{
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using Data;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Models;
  using Configuration;
  using System.Reflection;
  using System.Linq;
  using IdentityServer4.EntityFramework.DbContexts;
  using IdentityServer4.EntityFramework.Mappers;
  using Services;
  using System;
  using Microsoft.Extensions.Hosting;

  public class Startup
  {
    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    public Startup(
      IConfiguration configuration,
      IWebHostEnvironment environment)
    {
      Configuration = configuration;
      Environment = environment;
    }
   
    public void ConfigureServices(IServiceCollection services)
    {
      string databaseConnectionString = Configuration["Database:DefaultConnectionString"];
      string migrationAssemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

      services
          .AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(databaseConnectionString));

      services
          .AddIdentity<BejebejeUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      services
          .AddMvc()
          .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

      services
        .AddScoped<IDataSeederService, DataSeederService>();

      services
        .AddSingleton<Config>();

      services
        .AddOptions();

      services.Configure<IISOptions>(iis =>
      {
        iis.AuthenticationDisplayName = "Windows";
        iis.AutomaticAuthentication = false;
      });

      services
        .Configure<InitialSeedConfiguration>(Configuration.GetSection(nameof(InitialSeedConfiguration)))
        .Configure<InitialSeedConfiguration>(c =>
        {
          c.ConnectionString = databaseConnectionString;
        });

      services
        .Configure<EmailConfiguration>(Configuration.GetSection(nameof(EmailConfiguration)));

      services
        .Configure<InitialIdentityServerConfiguration>(Configuration.GetSection(nameof(InitialIdentityServerConfiguration)));

      services
        .Configure<IdentityOptions>(options =>
        {
          // password settings.
          options.Password.RequireDigit = false;
          options.Password.RequireLowercase = false;
          options.Password.RequireNonAlphanumeric = false;
          options.Password.RequireUppercase = false;
          options.Password.RequiredLength = 12;
          options.Password.RequiredUniqueChars = 0;

          // lockout settings.
          options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
          options.Lockout.MaxFailedAccessAttempts = 5;
          options.Lockout.AllowedForNewUsers = true;

          // user settings.
          options.User.AllowedUserNameCharacters =
          "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
          options.User.RequireUniqueEmail = true;

        });

      var builder = services
        .AddIdentityServer(options =>
        {
          options.Events.RaiseErrorEvents = true;
          options.Events.RaiseInformationEvents = true;
          options.Events.RaiseFailureEvents = true;
          options.Events.RaiseSuccessEvents = true;
        })
        .AddConfigurationStore(options =>
        {
          options.ConfigureDbContext = b => b.UseNpgsql(
            databaseConnectionString,
            sql => sql.MigrationsAssembly(migrationAssemblyName));
        })
        .AddOperationalStore(options =>
        {
          options.ConfigureDbContext = b => b.UseNpgsql(
            databaseConnectionString,
            sql => sql.MigrationsAssembly(migrationAssemblyName));

          options.EnableTokenCleanup = true;
        })
        .AddDeveloperSigningCredential()
        .AddAspNetIdentity<BejebejeUser>();

      services
        .AddAuthentication();

      services
        .AddScoped<IEmailService, EmailService>();
    }

    public void Configure(IApplicationBuilder app)
    {
      InitializeDatabase(app);

      if (Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.UseIdentityServer();

      app.Use(async (httpContent, next) =>
      {
        httpContent.Response.Headers.Add("Content-Security-Policy", "default-src 'self' *.googleapis.com *.gstatic.com; report-uri /cspreport");
        await next();
      });

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
      });
    }

    private void InitializeDatabase(IApplicationBuilder app)
    {
      using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
      {
        serviceScope
          .ServiceProvider
          .GetRequiredService<PersistedGrantDbContext>()
          .Database
          .Migrate();

        var context = serviceScope
          .ServiceProvider
          .GetRequiredService<ConfigurationDbContext>();

        Config identityServerConfiguration = serviceScope.ServiceProvider.GetRequiredService<Config>();

        context.Database.Migrate();

        if (!context.Clients.Any())
        {
          foreach (var client in identityServerConfiguration.GetClients())
          {
            context.Clients.Add(client.ToEntity());
          }
          context.SaveChanges();
        }

        if (!context.IdentityResources.Any())
        {
          foreach (var resource in identityServerConfiguration.GetIdentityResources())
          {
            context.IdentityResources.Add(resource.ToEntity());
          }
          context.SaveChanges();
        }

        if (!context.ApiResources.Any())
        {
          foreach (var resource in identityServerConfiguration.GetApis())
          {
            context.ApiResources.Add(resource.ToEntity());
          }
          context.SaveChanges();
        }
      }
    }
  }
}