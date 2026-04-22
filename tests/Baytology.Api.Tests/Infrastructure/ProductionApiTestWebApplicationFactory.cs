namespace Baytology.Api.Tests.Infrastructure;

public sealed class ProductionApiTestWebApplicationFactory : ApiTestWebApplicationFactory
{
    public ProductionApiTestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("AppSettings__AllowedOrigins__0", "https://frontend.baytology.test");
        Environment.SetEnvironmentVariable("Email__DeliveryMode", "Smtp");
        Environment.SetEnvironmentVariable("Email__SmtpHost", "smtp.test.local");
        Environment.SetEnvironmentVariable("Email__SmtpPort", "2525");
        Environment.SetEnvironmentVariable("Email__FromAddress", "no-reply@baytology.test");
        Environment.SetEnvironmentVariable("Email__FromDisplayName", "Baytology Test");
    }

    protected override string TestEnvironmentName => "Production";

    protected override IReadOnlyDictionary<string, string?> GetConfigurationOverrides()
    {
        var settings = new Dictionary<string, string?>(base.GetConfigurationOverrides())
        {
            ["AppSettings:AllowedOrigins:0"] = "https://frontend.baytology.test",
            ["Email:DeliveryMode"] = "Smtp",
            ["Email:SmtpHost"] = "smtp.test.local",
            ["Email:SmtpPort"] = "2525",
            ["Email:FromAddress"] = "no-reply@baytology.test",
            ["Email:FromDisplayName"] = "Baytology Test"
        };

        return settings;
    }
}
