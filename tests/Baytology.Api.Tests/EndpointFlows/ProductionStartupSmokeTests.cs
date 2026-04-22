using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class ProductionStartupSmokeTests(ProductionApiTestWebApplicationFactory factory)
    : IClassFixture<ProductionApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Production_configuration_boots_and_serves_core_public_and_identity_routes()
    {
        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/Properties")).StatusCode);

        var tokenResponse = await client.PostAsJsonAsync("/api/identity/token/generate", new
        {
            Email = TestSeedData.BuyerEmail,
            Password = TestSeedData.BuyerPassword
        });

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        var payload = await tokenResponse.ReadJsonAsync();
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("refreshToken").GetString()));
    }
}
