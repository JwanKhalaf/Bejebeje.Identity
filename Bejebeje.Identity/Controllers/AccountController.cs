using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Bejebeje.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Bejebeje.Identity.Filters;
using Bejebeje.Identity.ViewModels;
using Bejebeje.Identity.Extensions;
using Bejebeje.Identity.Options;
using Bejebeje.Identity.Services;
using System.Collections.Generic;

namespace Bejebeje.Identity.Controllers
{
  [SecurityHeaders]
  [AllowAnonymous]
  public class AccountController : Controller
  {
    private readonly UserManager<BejebejeUser> _userManager;
    private readonly SignInManager<BejebejeUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IEventService _events;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<BejebejeUser> userManager,
        SignInManager<BejebejeUser> signInManager,
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IAuthenticationSchemeProvider schemeProvider,
        IEventService events,
        IEmailService emailService)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _interaction = interaction;
      _clientStore = clientStore;
      _schemeProvider = schemeProvider;
      _events = events;
      _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Register()
    {
      RegisterViewModel registerViewModel = new RegisterViewModel();
      return View(registerViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        BejebejeUser user = new BejebejeUser
        {
          UserName = model.Email,
          Email = model.Email,
          DisplayUsername = model.Username
        };

        IdentityResult result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
          string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

          string callbackUrl = Url.Action(
            "ConfirmEmail",
            "Account",
            new { userId = user.Id, code = code },
            Request.Scheme);

          EmailRegistrationViewModel emailViewModel = new EmailRegistrationViewModel();
          emailViewModel.UserDisplayUsername = model.Username;
          emailViewModel.Code = callbackUrl;
          emailViewModel.UserEmailAddress = model.Email;

          await _emailService.SendRegistrationEmailAsync(emailViewModel);

          await _signInManager.SignInAsync(user, isPersistent: false);

          return Redirect("https://bejebeje.com");
        }

        foreach (IdentityError error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
      if (userId == null || code == null)
      {
        return RedirectToAction("Index", "Home");
      }

      BejebejeUser user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return NotFound($"Unable to load user with ID '{userId}'.");
      }

      IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

      if (!result.Succeeded)
      {
        throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
      }

      return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Entry point into the login workflow
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
      // build a model so we know what to show on the login page
      LoginViewModel loginViewModel = await BuildLoginViewModelAsync(returnUrl);

      if (loginViewModel.IsExternalLoginOnly)
      {
        // we only have one option for logging in and it's an external provider
        return RedirectToAction("Challenge", "External", new { provider = loginViewModel.ExternalLoginScheme, returnUrl });
      }

      return View(loginViewModel);
    }

    /// <summary>
    /// Handle postback from username/password login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model, string button)
    {
      // check if we are in the context of an authorization request
      AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

      // the user clicked the "cancel" button
      if (button != "login")
      {
        if (context != null)
        {
          // if the user cancels, send a result back into IdentityServer as if they 
          // denied the consent (even if this client does not require consent).
          // this will send back an access denied OIDC error response to the client.
          await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

          // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
          if (await _clientStore.IsPkceClientAsync(context.ClientId))
          {
            // if the client is PKCE then we assume it's native, so this change in how to
            // return the response is for better UX for the end user.
            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
          }

          return Redirect(model.ReturnUrl);
        }
        else
        {
          // since we don't have a valid context, then we just go back to the home page
          return Redirect("~/");
        }
      }

      if (ModelState.IsValid)
      {
        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);

        if (result.Succeeded)
        {
          BejebejeUser user = await _userManager.FindByNameAsync(model.Username);

          await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

          if (context != null)
          {
            if (await _clientStore.IsPkceClientAsync(context.ClientId))
            {
              // if the client is PKCE then we assume it's native, so this change in how to
              // return the response is for better UX for the end user.
              return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
            }

            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
            return Redirect(model.ReturnUrl);
          }

          // request for a local page
          if (Url.IsLocalUrl(model.ReturnUrl))
          {
            return Redirect(model.ReturnUrl);
          }
          else if (string.IsNullOrEmpty(model.ReturnUrl))
          {
            return Redirect("~/");
          }
          else
          {
            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");
          }
        }

        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
      }

      // something went wrong, show form with error
      LoginViewModel viewModel = await BuildLoginViewModelAsync(model);

      return View(viewModel);
    }


    /// <summary>
    /// Show logout page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
      // build a model so the logout page knows what to display
      LogoutViewModel viewModel = await BuildLogoutViewModelAsync(logoutId);

      if (viewModel.ShowLogoutPrompt == false)
      {
        // if the request for logout was properly authenticated from IdentityServer, then
        // we don't need to show the prompt and can just log the user out directly.
        return await Logout(viewModel);
      }

      return View(viewModel);
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
      // build a model so the logged out page knows what to display
      LoggedOutViewModel viewModel = await BuildLoggedOutViewModelAsync(model.LogoutId);

      if (User?.Identity.IsAuthenticated == true)
      {
        // delete local authentication cookie
        await _signInManager.SignOutAsync();

        // raise the logout event
        await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
      }

      // check if we need to trigger sign-out at an upstream identity provider
      if (viewModel.TriggerExternalSignout)
      {
        // build a return URL so the upstream provider will redirect back
        // to us after the user has logged out. this allows us to then
        // complete our single sign-out processing.
        string url = Url.Action("Logout", new { logoutId = viewModel.LogoutId });

        // this triggers a redirect to the external provider for sign-out
        return SignOut(new AuthenticationProperties { RedirectUri = url }, viewModel.ExternalAuthenticationScheme);
      }

      return View("LoggedOut", viewModel);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
      ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel();
      return View(viewModel);
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        BejebejeUser user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
          // Don't reveal that the user does not exist or is not confirmed
          return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // For more information on how to enable account confirmation and password reset please 
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        string code = await _userManager.GeneratePasswordResetTokenAsync(user);

        string callbackUrl = Url.Action(
          "ResetPassword",
          "Account",
          new { code },
          Request.Scheme);

        EmailForgotPasswordViewModel viewModel = new EmailForgotPasswordViewModel();
        viewModel.UserDisplayUsername = user.DisplayUsername;
        viewModel.Code = callbackUrl;
        viewModel.UserEmailAddress = user.Email;

        await _emailService.SendForgotPasswordEmailAsync(viewModel);

        return RedirectToAction("ForgotPasswordConfirmation");
      }

      return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
      return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string code = null)
    {
      if (code == null)
      {
        return BadRequest("A code must be supplied for password reset.");
      }
      else
      {
        ResetPasswordViewModel viewModel = new ResetPasswordViewModel
        {
          Code = code
        };

        return View(viewModel);
      }
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      BejebejeUser user = await _userManager.FindByEmailAsync(model.Email);

      if (user == null)
      {
        // Don't reveal that the user does not exist
        return RedirectToAction("ResetPasswordConfirmation");
      }

      IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

      if (result.Succeeded)
      {
        return RedirectToAction("ResetPasswordConfirmation");
      }

      foreach (IdentityError error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }

      return View(model);
    }

    /*****************************************/
    /* helper APIs for the AccountController */
    /*****************************************/
    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
      AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);

      if (context?.IdP != null)
      {
        // this is meant to short circuit the UI and only trigger the one external IdP
        return new LoginViewModel
        {
          EnableLocalLogin = false,
          ReturnUrl = returnUrl,
          Username = context?.LoginHint,
          ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
        };
      }

      IEnumerable<AuthenticationScheme> schemes = await _schemeProvider.GetAllSchemesAsync();

      List<ExternalProvider> providers = schemes
          .Where(x => x.DisplayName != null ||
                      (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
          )
          .Select(x => new ExternalProvider
          {
            DisplayName = x.DisplayName,
            AuthenticationScheme = x.Name
          }).ToList();

      bool allowLocal = true;

      if (context?.ClientId != null)
      {
        Client client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);

        if (client != null)
        {
          allowLocal = client.EnableLocalLogin;

          if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
          {
            providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
          }
        }
      }

      return new LoginViewModel
      {
        AllowRememberLogin = AccountOptions.AllowRememberLogin,
        EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
        ReturnUrl = returnUrl,
        Username = context?.LoginHint,
        ExternalProviders = providers.ToArray()
      };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
    {
      LoginViewModel viewModel = await BuildLoginViewModelAsync(model.ReturnUrl);
      viewModel.Username = model.Username;
      viewModel.RememberLogin = model.RememberLogin;

      return viewModel;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
    {
      LogoutViewModel viewModel = new LogoutViewModel
      {
        LogoutId = logoutId,
        ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt
      };

      if (User?.Identity.IsAuthenticated != true)
      {
        // if the user is not authenticated, then just show logged out page
        viewModel.ShowLogoutPrompt = false;

        return viewModel;
      }

      LogoutRequest context = await _interaction.GetLogoutContextAsync(logoutId);

      if (context?.ShowSignoutPrompt == false)
      {
        // it's safe to automatically sign-out
        viewModel.ShowLogoutPrompt = false;

        return viewModel;
      }

      // show the logout prompt. this prevents attacks where the user
      // is automatically signed out by another malicious web page.
      return viewModel;
    }

    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
    {
      // get context information (client name, post logout redirect URI and iframe for federated signout)
      LogoutRequest logout = await _interaction.GetLogoutContextAsync(logoutId);

      LoggedOutViewModel viewModel = new LoggedOutViewModel
      {
        AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
        PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
        ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
        SignOutIframeUrl = logout?.SignOutIFrameUrl,
        LogoutId = logoutId
      };

      if (User?.Identity.IsAuthenticated == true)
      {
        string idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

        if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
        {
          bool providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);

          if (providerSupportsSignout)
          {
            if (viewModel.LogoutId == null)
            {
              // if there's no current logout context, we need to create one
              // this captures necessary info from the current logged in user
              // before we signout and redirect away to the external IdP for signout
              viewModel.LogoutId = await _interaction.CreateLogoutContextAsync();
            }

            viewModel.ExternalAuthenticationScheme = idp;
          }
        }
      }

      return viewModel;
    }
  }
}