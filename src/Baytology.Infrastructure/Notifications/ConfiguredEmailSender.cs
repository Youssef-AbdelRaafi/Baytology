using System.Net;
using System.Net.Mail;
using System.Text;

using Baytology.Application.Common.Interfaces;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.Notifications;

public sealed class ConfiguredEmailSender(
    IOptions<EmailSettings> settings,
    ILogger<ConfiguredEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = settings.Value;

    public Task SendEmailConfirmationAsync(string email, string userId, string token)
    {
        var body = $$"""
            Baytology email confirmation

            UserId: {{userId}}
            Email: {{email}}
            ConfirmationToken: {{token}}
            """;

        return SendAsync(email, "Baytology email confirmation", body);
    }

    public Task SendPasswordResetAsync(string email, string token)
    {
        var body = $$"""
            Baytology password reset

            Email: {{email}}
            PasswordResetToken: {{token}}
            """;

        return SendAsync(email, "Baytology password reset", body);
    }

    private Task SendAsync(string recipientEmail, string subject, string body)
    {
        return _settings.DeliveryMode switch
        {
            EmailDeliveryMode.Smtp => SendViaSmtpAsync(recipientEmail, subject, body),
            _ => WritePickupMessageAsync(recipientEmail, subject, body)
        };
    }

    private async Task SendViaSmtpAsync(string recipientEmail, string subject, string body)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromDisplayName),
            Subject = subject,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };

        message.To.Add(recipientEmail);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
        }

        await client.SendMailAsync(message);
        logger.LogInformation("Sent email '{Subject}' to {RecipientEmail}.", subject, recipientEmail);
    }

    private async Task WritePickupMessageAsync(string recipientEmail, string subject, string body)
    {
        var pickupDirectory = ResolvePickupDirectory();
        Directory.CreateDirectory(pickupDirectory);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.txt";
        var filePath = Path.Combine(pickupDirectory, fileName);
        var content = $$"""
            To: {{recipientEmail}}
            Subject: {{subject}}

            {{body}}
            """;

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        logger.LogInformation("Wrote development email '{Subject}' to {FilePath}.", subject, filePath);
    }

    private string ResolvePickupDirectory()
    {
        if (Path.IsPathRooted(_settings.PickupDirectory))
            return _settings.PickupDirectory;

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _settings.PickupDirectory));
    }
}
