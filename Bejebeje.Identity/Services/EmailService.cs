namespace Bejebeje.Identity.Services
{
  using System.Threading.Tasks;
  using System.Net;
  using System.Net.Mail;
  using RazorLight;
  using Microsoft.Extensions.Options;
  using ViewModels;
  using Configuration;
  using System.IO;
  using System;
  using Microsoft.AspNetCore.Hosting;

  public class EmailService : IEmailService
  {
    private readonly IWebHostEnvironment _environment;

    private readonly EmailConfiguration _emailConfiguration;

    private readonly SmtpClient _smtpClient;

    public EmailService(
      IOptions<EmailConfiguration> emailConfiguration,
      IWebHostEnvironment environment)
    {
      _environment = environment;
      _emailConfiguration = emailConfiguration.Value;
      _smtpClient = new SmtpClient(_emailConfiguration.SmtpHost, _emailConfiguration.SmtpPort);

      _smtpClient.Credentials = new NetworkCredential(
        _emailConfiguration.SmtpServerUsername,
        _emailConfiguration.SmtpServerPassword);

      _smtpClient.EnableSsl = true;
    }

    public async Task SendRegistrationEmailAsync(EmailRegistrationViewModel emailViewModel)
    {
      string emailTemplateFolderPath = Path.Combine(_environment.ContentRootPath, "EmailTemplates");

      string registrationEmailTemplatePath = Path.Combine(emailTemplateFolderPath, "Registration.cshtml");

      Console.WriteLine($"The registration email template path is: {registrationEmailTemplatePath}");

      RazorLightEngine engine = new RazorLightEngineBuilder()
        .UseFilesystemProject(emailTemplateFolderPath)
        .UseMemoryCachingProvider()
        .Build();

      string emailHtmlBody = await engine.CompileRenderAsync(registrationEmailTemplatePath, emailViewModel);

      MailMessage mailMessage = new MailMessage(
        _emailConfiguration.OutgoingEmailAddress,
        emailViewModel.UserEmailAddress,
        "Welcome to Bêjebêje",
        emailHtmlBody);

      mailMessage.IsBodyHtml = true;

      string operationIdentity = "Registration email";

      SendEmailAsync(mailMessage, operationIdentity);
    }

    public async Task SendForgotPasswordEmailAsync(EmailForgotPasswordViewModel emailViewModel)
    {
      string emailTemplateFolderPath = Path.Combine(_environment.ContentRootPath, "EmailTemplates");

      string reportEmailTemplatePath = Path.Combine(emailTemplateFolderPath, "ForgotPassword.cshtml");

      RazorLightEngine engine = new RazorLightEngineBuilder()
        .UseFilesystemProject(emailTemplateFolderPath)
        .UseMemoryCachingProvider()
        .Build();

      string emailHtmlBody = await engine.CompileRenderAsync(reportEmailTemplatePath, emailViewModel);

      MailMessage mailMessage = new MailMessage(
        emailViewModel.UserEmailAddress,
        _emailConfiguration.OutgoingEmailAddress,
        "Forgotten Password",
        emailHtmlBody);

      mailMessage.IsBodyHtml = true;

      string operationIdentity = "Forgotten password email";

      SendEmailAsync(mailMessage, operationIdentity);
    }

    private void SendEmailAsync(MailMessage mailMessage, string operationIdentity)
    {
      _smtpClient.SendAsync(mailMessage, operationIdentity);
    }
  }
}