namespace Baytology.Infrastructure.Settings;

public enum EmailDeliveryMode
{
    FilePickup = 0,
    Smtp = 1
}

public class EmailSettings
{
    public EmailDeliveryMode DeliveryMode { get; set; } = EmailDeliveryMode.FilePickup;
    public string PickupDirectory { get; set; } = "runtime-logs/emails";
    public string FromAddress { get; set; } = "no-reply@baytology.local";
    public string FromDisplayName { get; set; } = "Baytology";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}
