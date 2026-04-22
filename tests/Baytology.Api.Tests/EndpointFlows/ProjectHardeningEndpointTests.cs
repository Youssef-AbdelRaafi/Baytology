using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class ProjectHardeningEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Agent_role_assignment_requires_clean_demotions_and_keeps_roster_semantically_consistent()
    {
        await factory.ResetDatabaseAsync();

        using var adminClient = factory.CreateAuthenticatedClient(TestSeedData.AdminUserId, TestSeedData.AdminEmail, "Admin");
        using var promotedAgentClient = factory.CreateAuthenticatedClient(TestSeedData.FreshBuyerUserId, TestSeedData.FreshBuyerEmail, "Agent");

        var promoteResponse = await adminClient.PostAsJsonAsync(
            $"/api/v1/Admin/users/{TestSeedData.FreshBuyerUserId}/assign-role",
            new
            {
                Role = "Agent"
            });

        Assert.Equal(HttpStatusCode.OK, promoteResponse.StatusCode);

        var updateDetailsResponse = await promotedAgentClient.PutAsJsonAsync("/api/v1/Agents/me", new
        {
            UserId = string.Empty,
            AgencyName = "Fresh Agent Office",
            LicenseNumber = "LIC-FRESH-2026",
            CommissionRate = 0.035
        });

        Assert.Equal(HttpStatusCode.OK, updateDetailsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync($"/api/v1/Agents/{TestSeedData.FreshBuyerUserId}")).StatusCode);

        using (var promotionScope = factory.Services.CreateScope())
        {
            var promotionContext = promotionScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var provisionedAgentDetailBeforeDemotion = await promotionContext.AgentDetails
                .SingleOrDefaultAsync(a => a.UserId == TestSeedData.FreshBuyerUserId);

            Assert.NotNull(provisionedAgentDetailBeforeDemotion);
            Assert.Equal("Fresh Agent Office", provisionedAgentDetailBeforeDemotion!.AgencyName);
            Assert.Equal(0.035m, provisionedAgentDetailBeforeDemotion.CommissionRate);
        }

        var agentsAfterPromotionResponse = await adminClient.GetAsync("/api/v1/Admin/agents");
        Assert.Equal(HttpStatusCode.OK, agentsAfterPromotionResponse.StatusCode);
        var agentsAfterPromotion = await agentsAfterPromotionResponse.ReadJsonAsync();
        Assert.Contains(
            agentsAfterPromotion.EnumerateArray(),
            item => item.GetProperty("userId").GetString() == TestSeedData.FreshBuyerUserId);

        var blockedDemotionResponse = await adminClient.PostAsJsonAsync(
            $"/api/v1/Admin/users/{TestSeedData.AgentUserId}/assign-role",
            new
            {
                Role = "Buyer"
            });

        Assert.Equal(HttpStatusCode.Conflict, blockedDemotionResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.GetAsync($"/api/v1/Agents/{TestSeedData.AgentUserId}")).StatusCode);

        var demoteFreshAgentResponse = await adminClient.PostAsJsonAsync(
            $"/api/v1/Admin/users/{TestSeedData.FreshBuyerUserId}/assign-role",
            new
            {
                Role = "Buyer"
            });

        Assert.Equal(HttpStatusCode.OK, demoteFreshAgentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await adminClient.GetAsync($"/api/v1/Agents/{TestSeedData.FreshBuyerUserId}")).StatusCode);

        var agentsAfterDemotionResponse = await adminClient.GetAsync("/api/v1/Admin/agents");
        Assert.Equal(HttpStatusCode.OK, agentsAfterDemotionResponse.StatusCode);
        var agentsAfterDemotion = await agentsAfterDemotionResponse.ReadJsonAsync();
        Assert.Contains(
            agentsAfterDemotion.EnumerateArray(),
            item => item.GetProperty("userId").GetString() == TestSeedData.AgentUserId);
        Assert.DoesNotContain(
            agentsAfterDemotion.EnumerateArray(),
            item => item.GetProperty("userId").GetString() == TestSeedData.FreshBuyerUserId);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Null(await context.AgentDetails.SingleOrDefaultAsync(a => a.UserId == TestSeedData.FreshBuyerUserId));
    }

    [Fact]
    public async Task Duplicate_profile_save_and_refund_requests_return_conflict_without_creating_duplicates()
    {
        await factory.ResetDatabaseAsync();

        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");
        using var freshBuyerClient = factory.CreateAuthenticatedClient(TestSeedData.FreshBuyerUserId, TestSeedData.FreshBuyerEmail, "Buyer");

        var createProfileResponse = await freshBuyerClient.PostAsJsonAsync("/api/v1/UserProfiles", new
        {
            UserId = string.Empty,
            DisplayName = "Fresh Buyer",
            AvatarUrl = "https://images.test/fresh-buyer.png",
            Bio = "Fresh buyer bio",
            PhoneNumber = "01000000003",
            PreferredContactMethod = 0
        });
        Assert.Equal(HttpStatusCode.Created, createProfileResponse.StatusCode);

        var duplicateProfileResponse = await freshBuyerClient.PostAsJsonAsync("/api/v1/UserProfiles", new
        {
            UserId = string.Empty,
            DisplayName = "Fresh Buyer Duplicate",
            AvatarUrl = "https://images.test/fresh-buyer-duplicate.png",
            Bio = "Duplicate profile attempt",
            PhoneNumber = "01000000003",
            PreferredContactMethod = 0
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicateProfileResponse.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await buyerClient.PostAsync($"/api/v1/Properties/{factory.SeedData.SavablePropertyId}/save", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await buyerClient.PostAsync($"/api/v1/Properties/{factory.SeedData.SavablePropertyId}/save", null)).StatusCode);

        var requestRefundResponse = await buyerClient.PostAsJsonAsync("/api/v1/Payments/refunds", new
        {
            PaymentId = factory.SeedData.RefundablePaymentId,
            RequestedBy = string.Empty,
            Reason = "Need a refund",
            Amount = 5000m
        });
        Assert.Equal(HttpStatusCode.OK, requestRefundResponse.StatusCode);

        var duplicateRefundResponse = await buyerClient.PostAsJsonAsync("/api/v1/Payments/refunds", new
        {
            PaymentId = factory.SeedData.RefundablePaymentId,
            RequestedBy = string.Empty,
            Reason = "Duplicate refund attempt",
            Amount = 5000m
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicateRefundResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(1, await context.UserProfiles.CountAsync(p => p.UserId == TestSeedData.FreshBuyerUserId));
        Assert.Equal(1, await context.SavedProperties.CountAsync(sp => sp.UserId == TestSeedData.BuyerUserId && sp.PropertyId == factory.SeedData.SavablePropertyId));
        Assert.Equal(1, await context.RefundRequests.CountAsync(r => r.PaymentId == factory.SeedData.RefundablePaymentId));
    }

    [Fact]
    public async Task Create_property_persists_building_structure_fields()
    {
        await factory.ResetDatabaseAsync();

        using var agentClient = factory.CreateAuthenticatedClient(TestSeedData.AgentUserId, TestSeedData.AgentEmail, "Agent");

        var createResponse = await agentClient.PostAsJsonAsync("/api/v1/Properties", new
        {
            Title = "Structure Aware Listing",
            Description = "Tracks floor and total floors.",
            PropertyType = 0,
            ListingType = 1,
            Price = 1800000m,
            Area = 165m,
            Bedrooms = 3,
            Bathrooms = 2,
            Floor = 7,
            TotalFloors = 12,
            AddressLine = "Business Street",
            City = "Cairo",
            District = "New Cairo",
            ZipCode = "11835",
            Latitude = 30.1234567m,
            Longitude = 31.4567890m,
            ImageUrls = new[] { "https://images.test/structure-aware.jpg" }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdBody = await createResponse.ReadJsonAsync();
        var propertyId = createdBody.GetProperty("id").GetGuid();

        var getResponse = await agentClient.GetAsync($"/api/v1/Properties/{propertyId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var property = await getResponse.ReadJsonAsync();
        Assert.Equal(7, property.GetProperty("floor").GetInt32());
        Assert.Equal(12, property.GetProperty("totalFloors").GetInt32());
    }
}
