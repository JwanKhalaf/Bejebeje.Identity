namespace Bejebeje.Identity.Configuration
{
  public class EmailConfiguration
  {
    public string ApplicationOutgoingEmailAddress { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpServerUsername { get; set; }
    public string SmtpServerPassword { get; set; }
  }
}