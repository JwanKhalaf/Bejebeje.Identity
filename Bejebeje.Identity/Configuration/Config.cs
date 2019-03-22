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
            ClientUri = "http://identityserver.io",
            AllowedGrantTypes = GrantTypes.Implicit,
            AllowAccessTokensViaBrowser = true,
            RedirectUris =
            {
                "http://localhost:5002/index.html",
                "http://localhost:5002/callback.html",
                "http://localhost:5002/silent.html",
                "http://localhost:5002/popup.html",
            },
            PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
            AllowedCorsOrigins = { "http://localhost:5002" },
            AllowedScopes = { "openid", "profile", "api1" }
        }
      };
    }
  }
}
