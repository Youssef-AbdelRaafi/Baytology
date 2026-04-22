using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class AgentEndpointFlowTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Agent_endpoints_work_end_to_end()
    {
        await factory.ResetDatabaseAsync();

        using var agentClient = factory.CreateAuthenticatedClient(TestSeedData.AgentUserId, TestSeedData.AgentEmail, "Agent");

        Assert.Equal(HttpStatusCode.OK, (await agentClient.PutAsJsonAsync("/api/v1/Agents/me", new
        {
            UserId = "",
            AgencyName = "Baytology Estates Updated",
            LicenseNumber = "LIC-2026-UPDATED",
            CommissionRate = 0.04
        })).StatusCode);

        var createPropertyResponse = await agentClient.PostAsJsonAsync("/api/v1/Properties", new
        {
            AgentUserId = "",
            Title = "New Agent Listing",
            Description = "Freshly listed property",
            PropertyType = 0,
            ListingType = 1,
            Price = 18000,
            Area = 175,
            Bedrooms = 3,
            Bathrooms = 2,
            Floor = 4,
            TotalFloors = 10,
            AddressLine = "Test Address",
            City = "Cairo",
            District = "New Cairo",
            ZipCode = "11835",
            Latitude = 30.01,
            Longitude = 31.45,
            HasParking = true,
            HasPool = false,
            HasGym = true,
            HasElevator = true,
            HasSecurity = true,
            HasBalcony = true,
            HasGarden = false,
            HasCentralAC = true,
            FurnishingStatus = 2,
            ViewType = 3,
            ImageUrls = new[] { "https://images.test/new-listing-1.jpg" }
        });
        Assert.Equal(HttpStatusCode.Created, createPropertyResponse.StatusCode);

        var createdPropertyId = (await createPropertyResponse.ReadJsonAsync()).GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.OK, (await agentClient.PutAsJsonAsync($"/api/v1/Properties/{createdPropertyId}", new
        {
            PropertyId = Guid.Empty,
            AgentUserId = "",
            Title = "Updated Agent Listing",
            Description = "Updated description",
            PropertyType = 0,
            ListingType = 1,
            Price = 19500,
            Area = 180,
            Bedrooms = 4,
            Bathrooms = 3,
            Floor = 5,
            TotalFloors = 12,
            AddressLine = "Updated Address",
            City = "Cairo",
            District = "New Cairo",
            ZipCode = "11835",
            Latitude = 30.02,
            Longitude = 31.46,
            IsFeatured = true,
            HasParking = true,
            HasPool = true,
            HasGym = true,
            HasElevator = true,
            HasSecurity = true,
            HasBalcony = true,
            HasGarden = true,
            HasCentralAC = true,
            FurnishingStatus = 2,
            ViewType = 3
        })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await agentClient.PostAsJsonAsync(
            $"/api/v1/Properties/{createdPropertyId}/images",
            new
            {
                ImageUrls = new[] { "https://images.test/extra-1.jpg", "https://images.test/extra-2.jpg" }
            })).StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(3, await context.PropertyImages.CountAsync(i => i.PropertyId == createdPropertyId));
        }

        Assert.Equal(HttpStatusCode.OK, (await agentClient.PatchAsync(
            $"/api/v1/Bookings/{factory.SeedData.PendingBookingId}/status",
            JsonContent.Create(new { Status = 2 }))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await agentClient.DeleteAsync($"/api/v1/Properties/{createdPropertyId}")).StatusCode);

        using var finalScope = factory.Services.CreateScope();
        var finalContext = finalScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var agentDetail = await finalContext.AgentDetails.FirstOrDefaultAsync(a => a.UserId == TestSeedData.AgentUserId);
        var cancelledBooking = await finalContext.Bookings.FindAsync(factory.SeedData.PendingBookingId);
        var deletedProperty = await finalContext.Properties.FindAsync(createdPropertyId);

        Assert.NotNull(agentDetail);
        Assert.Equal("Baytology Estates Updated", agentDetail!.AgencyName);
        Assert.Equal(0.04m, agentDetail.CommissionRate);
        Assert.NotNull(cancelledBooking);
        Assert.Equal(BookingStatus.Cancelled, cancelledBooking!.Status);
        Assert.Null(deletedProperty);
        Assert.Equal(0, await finalContext.PropertyImages.CountAsync(i => i.PropertyId == createdPropertyId));
    }
}
