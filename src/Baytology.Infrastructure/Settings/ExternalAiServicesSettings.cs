namespace Baytology.Infrastructure.Settings;

public class ExternalAiServicesSettings
{
    public bool ChatbotEnabled { get; set; }
    public string ChatbotBaseUrl { get; set; } = string.Empty;
    public bool RecommendationEnabled { get; set; }
    public string RecommendationBaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
