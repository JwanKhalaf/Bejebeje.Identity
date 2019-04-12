using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using RazorLight;
using Microsoft.Extensions.Options;
using Bejebeje.Identity.ViewModels;
using Bejebeje.Identity.Configuration;
using System.IO;
using System;

namespace Bejebeje.Identity.Services
{
  public class EmailService : IEmailService
  {
    private EmailConfiguration _emailConfiguration;

    public EmailService(IOptions<EmailConfiguration> emailConfiguration)
    {
      _emailConfiguration = emailConfiguration.Value;
    }

    public async Task SendEmailAsync(EmailRegistrationViewModel emailViewModel)
    {

      string emailTemplateFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");

      string reportEmailTemplatePath = Path.Combine(emailTemplateFolderPath, "Registration.cshtml");

      RazorLightEngine engine = new RazorLightEngineBuilder()
        .UseFilesystemProject(emailTemplateFolderPath)
        .UseMemoryCachingProvider()
        .Build();

      string emailHtmlBody = await engine.CompileRenderAsync(reportEmailTemplatePath, emailViewModel);

      SmtpClient smtpClient = new SmtpClient(_emailConfiguration.SmtpHost, _emailConfiguration.SmtpPort);

      smtpClient.Credentials = new NetworkCredential(
        _emailConfiguration.SmtpServerUsername,
        _emailConfiguration.SmtpServerPassword);

      MailMessage mailMessage = new MailMessage(
        emailViewModel.UserEmailAddress,
        _emailConfiguration.OutgoingEmailAddress,
        "Welcome to Bejebeje",
        emailHtmlBody);

      mailMessage.IsBodyHtml = true;

      smtpClient.EnableSsl = true;

      smtpClient.Send(mailMessage);
    }
  }
}