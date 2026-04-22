using Microsoft.Extensions.Configuration;

namespace Baytology.Api.Tests.Infrastructure;

public sealed class ProductionConfigurationTests
{
    [Fact]
    public void Production_configuration_binds_cors_from_appsettings_section()
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
            .AddJsonFile("appsettings.Production.json", optional: false)
            .Build();

        Assert.True(configuration.GetSection("AppSettings").Exists());
        Assert.Empty(configuration.GetSection("AppSettings:AllowedOrigins").GetChildren());
        Assert.False(configuration.GetSection("Cors").Exists());
    }
}
