﻿using System.ComponentModel.DataAnnotations;

namespace Bejebeje.Identity.Models
{
  public class LoginInputModel
  {
    [Required]
    [Display (Name = "Email")]
    [EmailAddress]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    public bool RememberLogin { get; set; }
    public string ReturnUrl { get; set; }
  }
}