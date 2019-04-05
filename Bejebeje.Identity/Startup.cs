using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bejebeje.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bejebeje.Identity.Models;
using Bejebeje.Identity.Configuration;
using System.Reflection;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

namespace Bejebeje.Identity
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public IHostingEnvironment Environment { get; }

    public Startup(
      IConfiguration configuration,
      IHostingEnvironment environment)
    {
      Configuration = configuration;
      Environment = environment;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      string databaseConnectionString = Configuration["Database:DefaultConnectionString"];
      string migrationAssemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

      services
          .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(databaseConnectionString));

      services
          .AddIdentity<BejebejeUser, IdentityRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      services
          .AddMvc()
          .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      services
        .AddSingleton<DataSeeder>();
      
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
        .Configure<InitialIdentityServerConfiguration>(Configuration.GetSection(nameof(InitialIdentityServerConfiguration)));

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
        .AddAuthentication()
        .AddGoogle(options =>
        {
          // register your IdentityServer with Google at https://console.developers.google.com
          // enable the Google+ API
          // set the redirect URI to http://localhost:5000/signin-google
          options.ClientId = "copy client ID from Google here";
          options.ClientSecret = "copy client secret from Google here";
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
      // this will do the initial DB population
      InitializeDatabase(app);

      if (Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }

      app.UseStaticFiles();
      app.UseIdentityServer();
      app.UseMvcWithDefaultRoute();
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