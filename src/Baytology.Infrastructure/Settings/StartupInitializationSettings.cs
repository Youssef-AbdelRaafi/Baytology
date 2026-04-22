namespace Baytology.Infrastructure.Settings;

public class StartupInitializationSettings
{
    public bool Enabled { get; set; } = true;
    public bool SeedPropertyCsvOnStartup { get; set; }
}
