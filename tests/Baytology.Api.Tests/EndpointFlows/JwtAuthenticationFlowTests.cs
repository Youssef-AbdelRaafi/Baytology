using System.Net;
using System.Net.Http.Headers;

using Baytology.Api.Tests.Infrastructure;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class JwtAuthenticationFlowTests(JwtApiTestWebApplicationFactory factory)
    : IClassFixture<JwtApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Jwt_tokens_from_identity_endpoint_authorize_real_bearer_requests()
    {
        await factory.ResetDatabaseAsync();

        var accessToken = await factory.CreateAccessTokenAsync(TestSeedData.BuyerEmail, TestSeedData.BuyerPassword);

        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/identity/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/Bookings")).StatusCode);
    }

    [Fact]
    public async Task Jwt_tokens_preserve_role_authorization_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        var buyerToken = await factory.CreateAccessTokenAsync(TestSeedData.BuyerEmail, TestSeedData.BuyerPassword);
        var adminToken = await factory.CreateAccessTokenAsync(TestSeedData.AdminEmail, TestSeedData.AdminPassword);

        using var buyerClient = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        buyerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", buyerToken);

        using var adminClient = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        Assert.Equal(HttpStatusCode.Forbidden, (await buyerClient.GetAsync("/api/v1/Admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync("/api/v1/Admin/users")).StatusCode);
    }
}
