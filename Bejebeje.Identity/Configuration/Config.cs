using IdentityServer4.Models;
using System.Collections.Generic;

namespace Bejebeje.Identity.Configuration
{
  public static class Config
  {
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
      return new IdentityResource[]
      {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
      };
    }

    public static IEnumerable<ApiResource> GetApis()
    {
      return new ApiResource[]
      {
        new ApiResource("bejebeje_api", "Bejebeje API")
      };
    }

    public static IEnumerable<Client> GetClients()
    {
      return new[]
      {
        new Client
        {
            ClientId = "bejebeje_react-spa",
            ClientName = "Bejebeje ReactJS SPA Client",
            AllowedGrantTypes = GrantTypes.Implicit,
            AllowAccessTokensViaBrowser = true,
            RequireConsent = false,
            RedirectUris = { "https://bejebeje.com/callback" },
            PostLogoutRedirectUris = { "https://bejebeje.com" },
            AllowedCorsOrigins = { "https://bejebeje.com" },
            AllowedScopes = { "openid", "profile", "bejebeje_api" }
        },
        new Client
        {
            ClientId = "bejebeje_react-local",
            ClientName = "Bejebeje ReactJS SPA Client",
            AllowedGrantTypes = GrantTypes.Implicit,
            AllowAccessTokensViaBrowser = true,
            RequireConsent = false,
            RedirectUris = { "https://localhost:1234/callback" },
            PostLogoutRedirectUris = { "https://localhost:1234" },
            AllowedCorsOrigins = { "https://localhost:1234" },
            AllowedScopes = { "openid", "profile", "bejebeje_api" }
        }
      };
    }
  }
}
