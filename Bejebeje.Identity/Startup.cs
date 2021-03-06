﻿namespace Bejebeje.Identity
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
  using IdentityServer4.Models;
  using Joonasw.AspNetCore.SecurityHeaders;
  using Microsoft.AspNetCore.HttpOverrides;
  using Microsoft.AspNetCore.StaticFiles;
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

      services
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
        .AddAspNetIdentity<BejebejeUser>()
        .AddProfileService<ProfileService>();

      services
        .AddAuthentication();

      services
        .AddScoped<IEmailService, EmailService>();
    }

    public void Configure(
      IApplicationBuilder app,
      ApplicationDbContext context)
    {
      context.Database.Migrate();

      ForwardedHeadersOptions forwardedHeadersOptions = new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
      };

      forwardedHeadersOptions.KnownNetworks.Clear();
      forwardedHeadersOptions.KnownProxies.Clear();

      app.UseForwardedHeaders(forwardedHeadersOptions);

      InitializeDatabase(app);

      if (Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");

        HstsBuilderExtensions.UseHsts(app);
      }

      FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

      fileExtensionContentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

      app.UseStaticFiles(new StaticFileOptions()
      {
        ContentTypeProvider = fileExtensionContentTypeProvider
      });

      app.UseCsp(csp =>
      {
        csp.AllowFonts
          .FromSelf()
          .From("fonts.googleapis.com")
          .From("fonts.gstatic.com");
      });

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseIdentityServer();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
      });
    }

    private void InitializeDatabase(IApplicationBuilder app)
    {
      using IServiceScope serviceScope = app
        .ApplicationServices
        .GetService<IServiceScopeFactory>()
        .CreateScope();

      serviceScope
        .ServiceProvider
        .GetRequiredService<PersistedGrantDbContext>()
        .Database
        .Migrate();

      ConfigurationDbContext context = serviceScope
        .ServiceProvider
        .GetRequiredService<ConfigurationDbContext>();

      Config identityServerConfiguration = serviceScope.ServiceProvider.GetRequiredService<Config>();

      context.Database.Migrate();

      if (!context.Clients.Any())
      {
        foreach (Client client in identityServerConfiguration.GetClients())
        {
          context.Clients.Add(client.ToEntity());
        }
        context.SaveChanges();
      }

      if (!context.IdentityResources.Any())
      {
        foreach (IdentityResource resource in identityServerConfiguration.GetIdentityResources())
        {
          context.IdentityResources.Add(resource.ToEntity());
        }

        context.SaveChanges();
      }
    }
  }
}