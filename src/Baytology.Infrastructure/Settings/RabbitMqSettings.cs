namespace Baytology.Infrastructure.Settings;

public class RabbitMqSettings
{
    public bool Enabled { get; set; } = true;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SearchQueueName { get; set; } = "ai-search";
    public string RecommendationQueueName { get; set; } = "recommendations";
    public string PropertyIndexQueueName { get; set; } = "property-index";
    public string UserHistoryQueueName { get; set; } = "user-history";
}
