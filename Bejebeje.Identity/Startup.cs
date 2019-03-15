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
using System;

namespace Bejebeje.Identity
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public IHostingEnvironment Environment { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(
      IServiceCollection services,
      IHostingEnvironment environment)
    {
      services
          .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

      services
          .AddIdentity<BejebejeUser, IdentityUser>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      services
          .AddMvc()
          .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      services.Configure<IISOptions>(iis =>
      {
        iis.AuthenticationDisplayName = "Windows";
        iis.AutomaticAuthentication = false;
      });

      var builder = services
        .AddIdentityServer(options => 
        {
          options.Events.RaiseErrorEvents = true;
          options.Events.RaiseInformationEvents = true;
          options.Events.RaiseFailureEvents = true;
          options.Events.RaiseSuccessEvents = true;
        })
        .AddInMemoryIdentityResources(Config.GetIdentityResources())
        .AddInMemoryApiResources(Config.GetApis())
        .AddInMemoryClients(Config.GetClients())
        .AddAspNetIdentity<BejebejeUser>();

      if (Environment.IsDevelopment())
      {
        builder.AddDeveloperSigningCredential();
      }
      else
      {
        throw new Exception("need to configure key material");
      }

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
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseCookiePolicy();

      app.UseAuthentication();

      app.UseMvc(routes =>
      {
        routes.MapRoute(
          name: "default",
          template: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
