namespace Bejebeje.Identity.Configuration
{
  using IdentityServer4.Models;
  using System.Collections.Generic;
  using IdentityServer4;
  using Microsoft.Extensions.Options;

  public class Config
  {
    private InitialIdentityServerConfiguration IdentityServerConfiguration { get; set; }

    public Config(IOptions<InitialIdentityServerConfiguration> initialIdentityServerConfiguration)
    {
      IdentityServerConfiguration = initialIdentityServerConfiguration.Value;
    }

    public IEnumerable<IdentityResource> GetIdentityResources()
    {
      return new IdentityResource[]
      {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
      };
    }

    public IEnumerable<Client> GetClients()
    {
      return new[]
      {
        new Client
        {
          ClientId = IdentityServerConfiguration.FrontendClientId,
          ClientName = "bejebeje frontend mvc client",
          ClientSecrets = { new Secret(IdentityServerConfiguration.FrontendClientSecret.Sha256()) },
          AllowedGrantTypes = GrantTypes.Code,
          RequireConsent = false,
          RequirePkce = true,
          // where to redirect to after login
          RedirectUris = { $"{IdentityServerConfiguration.FrontendEndpoint}/signin-oidc" },
          // where to redirect to after logout
          PostLogoutRedirectUris = { $"{IdentityServerConfiguration.FrontendEndpoint}/signout-callback-oidc" },
          AllowedScopes = new List<string>
          {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
          },
          AllowedCorsOrigins = new List<string>
          {
            IdentityServerConfiguration.FrontendEndpoint
          },
        },
        new Client
        {
          ClientId = IdentityServerConfiguration.AdminClientId,
          ClientName = "bejebeje admin mvc client",
          ClientSecrets = { new Secret(IdentityServerConfiguration.AdminClientSecret.Sha256()) },
          AllowedGrantTypes = GrantTypes.Code,
          RequireConsent = false,
          RequirePkce = true,
          // where to redirect to after login
          RedirectUris = { $"{IdentityServerConfiguration.AdminEndpoint}/signin-oidc" },
          // where to redirect to after logout
          PostLogoutRedirectUris = { $"{IdentityServerConfiguration.AdminEndpoint}/signout-callback-oidc" },
          AllowedScopes = new List<string>
          {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
          },
        },
      };
    }
  }
}
