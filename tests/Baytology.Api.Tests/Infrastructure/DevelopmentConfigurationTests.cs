using Microsoft.Extensions.Configuration;

namespace Baytology.Api.Tests.Infrastructure;

public sealed class DevelopmentConfigurationTests
{
    [Fact]
    public void Development_configuration_keeps_property_csv_seed_opt_in()
    {
        var apiProjectDirectory = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "Baytology.Api"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();

        Assert.False(configuration.GetValue<bool>("StartupInitialization:SeedPropertyCsvOnStartup"));
    }
}
