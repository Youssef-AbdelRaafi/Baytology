namespace Baytology.Infrastructure.Settings;

public class PaymobSettings
{
    public bool EnableLocalSimulation { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://accept.paymob.com/v1/intention/";
    public int IntegrationId { get; set; }
    public string WebhookToken { get; set; } = string.Empty;
    public string WebhookTokenHeaderName { get; set; } = "X-Webhook-Token";
    public string WebhookTokenQueryParameterName { get; set; } = "token";
}
