namespace Baytology.Infrastructure.Settings;

public class AppSettings
{
    public string CorsPolicyName { get; set; } = "Baytology";
    public string[]? AllowedOrigins { get; set; }
    public int DefaultPageNumber { get; set; } = 1;
    public int DefaultPageSize { get; set; } = 10;
}
