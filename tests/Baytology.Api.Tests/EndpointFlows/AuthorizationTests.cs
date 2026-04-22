using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class AuthorizationTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Secured_endpoints_enforce_authentication_and_roles()
    {
        await factory.ResetDatabaseAsync();

        using var anonymousClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymousClient.GetAsync("/api/v1/Bookings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await buyerClient.GetAsync("/api/v1/Admin/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await buyerClient.PostAsJsonAsync("/api/v1/Properties", new
        {
            Title = "Buyer cannot create this",
            Description = "Nope",
            PropertyType = 0,
            ListingType = 0,
            Price = 1000,
            Area = 100,
            Bedrooms = 2,
            Bathrooms = 1
        })).StatusCode);
    }
}
