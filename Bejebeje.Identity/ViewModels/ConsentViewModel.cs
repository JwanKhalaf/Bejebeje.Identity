﻿using System.Collections.Generic;
using Bejebeje.Identity.Models;

namespace Bejebeje.Identity.ViewModels
{
  public class ConsentViewModel : ConsentInputModel
  {
    public string ClientName { get; set; }
    public string ClientUrl { get; set; }
    public string ClientLogoUrl { get; set; }
    public bool AllowRememberConsent { get; set; }

    public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
    public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
  }
}
