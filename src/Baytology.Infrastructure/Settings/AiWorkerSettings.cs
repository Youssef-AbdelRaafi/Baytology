namespace Baytology.Infrastructure.Settings;

public class AiWorkerSettings
{
    public string ServiceToken { get; set; } = string.Empty;
    public string ServiceTokenHeaderName { get; set; } = "X-AI-Service-Token";
}
