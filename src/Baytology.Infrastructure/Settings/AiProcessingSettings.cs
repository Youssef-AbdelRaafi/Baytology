namespace Baytology.Infrastructure.Settings;

public class AiProcessingSettings
{
    public bool EnableInProcessFallback { get; set; }
    public int DefaultSearchResultLimit { get; set; } = 10;
    public int DefaultRecommendationResultLimit { get; set; } = 10;
    public bool EnableDelayedFallbackRecovery { get; set; } = true;
    public int ExternalWorkerGracePeriodSeconds { get; set; } = 10;
}
