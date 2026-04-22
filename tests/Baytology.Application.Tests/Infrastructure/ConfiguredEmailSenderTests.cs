using Baytology.Infrastructure.Notifications;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Baytology.Application.Tests.Infrastructure;

public sealed class ConfiguredEmailSenderTests
{
    [Fact]
    public async Task FilePickup_mode_writes_email_payload_to_disk()
    {
        var pickupDirectory = Path.Combine(Path.GetTempPath(), $"baytology-email-tests-{Guid.NewGuid():N}");

        try
        {
            var sender = new ConfiguredEmailSender(
                Options.Create(new EmailSettings
                {
                    DeliveryMode = EmailDeliveryMode.FilePickup,
                    PickupDirectory = pickupDirectory,
                    FromAddress = "no-reply@baytology.local",
                    FromDisplayName = "Baytology"
                }),
                NullLogger<ConfiguredEmailSender>.Instance);

            await sender.SendPasswordResetAsync("buyer@test.local", "reset-token-value");

            var files = Directory.GetFiles(pickupDirectory);
            var file = Assert.Single(files);
            var body = await File.ReadAllTextAsync(file);

            Assert.Contains("buyer@test.local", body);
            Assert.Contains("reset-token-value", body);
        }
        finally
        {
            if (Directory.Exists(pickupDirectory))
                Directory.Delete(pickupDirectory, recursive: true);
        }
    }
}
