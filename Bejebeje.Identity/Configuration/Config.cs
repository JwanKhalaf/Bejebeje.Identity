using IdentityServer4.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Bejebeje.Identity.Configuration
{
  public class Config
  {
    private InitialIdentityServerConfiguration identityServerConfiguration { get; set; }

    public Config(IOptions<InitialIdentityServerConfiguration> initialIdentityServerConfiguration)
    {
      identityServerConfiguration = initialIdentityServerConfiguration.Value;
    }

    public IEnumerable<IdentityResource> GetIdentityResources()
    {
      return new IdentityResource[]
      {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
      };
    }

    public IEnumerable<ApiResource> GetApis()
    {
      return new ApiResource[]
      {
        new ApiResource(identityServerConfiguration.ApiName, "Bejebeje API"),
        new ApiResource("bejebeje-api-local", "Bejebeje API")
      };
    }

    public IEnumerable<Client> GetClients()
    {
      return new[]
      {
        new Client
        {
          ClientId = identityServerConfiguration.FrontendClientId,
          ClientName = "Bejebeje ReactJS SPA Client",
          AllowedGrantTypes = GrantTypes.Implicit,
          AllowAccessTokensViaBrowser = true,
          RequireConsent = false,
          RedirectUris = { "https://bejebeje.com/callback" },
          PostLogoutRedirectUris = { "https://bejebeje.com" },
          AllowedCorsOrigins = { "https://bejebeje.com" },
          AllowedScopes = { "openid", "profile", identityServerConfiguration.ApiName }
        },
        new Client
        {
          ClientId = "bejebeje-react-local",
          ClientName = "Bejebeje ReactJS SPA Client",
          AllowedGrantTypes = GrantTypes.Implicit,
          AllowAccessTokensViaBrowser = true,
          RequireConsent = false,
          RedirectUris = { "http://localhost:1234/callback" },
          PostLogoutRedirectUris = { "http://localhost:1234" },
          AllowedCorsOrigins = { "http://localhost:1234" },
          AllowedScopes = { "openid", "profile", "bejebeje-api-local" }
        }
      };
    }
  }
}
