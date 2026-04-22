using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;
using Baytology.Domain.AISearch;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Properties;
using Baytology.Infrastructure.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class InternalAiWorkerEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task Internal_ai_worker_endpoints_require_the_service_token()
    {
        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/internal/ai/property-mappings/lookup", new
        {
            items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Internal_ai_worker_lookup_can_match_properties_by_exact_source_listing_url()
    {
        await factory.ResetDatabaseAsync();

        Guid propertyId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var property = Property.Create(
                TestSeedData.AgentUserId,
                "Exact Mapping Apartment",
                "Description",
                PropertyType.Apartment,
                ListingType.Sale,
                2100000m,
                175m,
                3,
                2,
                "Cairo",
                "New Cairo",
                "https://example.test/listings/exact-mapping").Value;

            property.ClearDomainEvents();
            context.Properties.Add(property);
            await context.SaveChangesAsync();
            propertyId = property.Id;
        }

        using var client = CreateWorkerClient();
        var response = await client.PostAsJsonAsync("/api/internal/ai/property-mappings/lookup", new
        {
            items = new[]
            {
                new
                {
                    sourceListingUrl = "https://example.test/listings/exact-mapping/",
                    title = "Different title should not matter when URL is exact"
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.ReadJsonAsync();
        var result = payload.GetProperty("results")[0];
        Assert.Equal(propertyId.ToString(), result.GetProperty("propertyId").GetGuid().ToString());
        Assert.Equal("SourceListingUrl", result.GetProperty("matchSource").GetString());
    }

    [Fact]
    public async Task Internal_ai_worker_search_resolution_completes_pending_requests()
    {
        await factory.ResetDatabaseAsync();

        Guid searchRequestId;
        Guid propertyId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var property = Property.Create(
                TestSeedData.AgentUserId,
                "Worker Resolved Apartment",
                "Description",
                PropertyType.Apartment,
                ListingType.Sale,
                1900000m,
                160m,
                3,
                2,
                "Giza",
                "Sheikh Zayed").Value;
            property.ClearDomainEvents();

            var searchRequest = SearchRequest.Create(TestSeedData.BuyerUserId, SearchInputType.Text, SearchEngine.Hybrid, "worker-callback").Value;
            searchRequest.ClearDomainEvents();

            context.Properties.Add(property);
            context.SearchRequests.Add(searchRequest);
            await context.SaveChangesAsync();

            propertyId = property.Id;
            searchRequestId = searchRequest.Id;
        }

        using var client = CreateWorkerClient();
        var response = await client.PostAsJsonAsync($"/api/internal/ai/search/{searchRequestId:D}/resolve", new
        {
            isSuccessful = true,
            results = new[]
            {
                new
                {
                    propertyId,
                    rank = 1,
                    relevanceScore = 0.98f,
                    scoreSource = "PythonWorker",
                    snapshotTitle = "Worker snapshot",
                    snapshotPrice = 1900000m,
                    snapshotCity = "Giza",
                    snapshotStatus = "Available"
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedRequest = await verificationContext.SearchRequests.FindAsync(searchRequestId);
        var savedResult = await verificationContext.SearchResults.SingleAsync(result => result.SearchRequestId == searchRequestId);

        Assert.NotNull(savedRequest);
        Assert.Equal(RequestStatus.Completed, savedRequest!.Status);
        Assert.Equal(propertyId, savedResult.PropertyId);
        Assert.Equal("PythonWorker", savedResult.ScoreSource);
        Assert.Equal("Worker snapshot", savedResult.SnapshotTitle);
    }

    private HttpClient CreateWorkerClient()
    {
        var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Add(TestSeedData.AiWorkerServiceTokenHeaderName, TestSeedData.AiWorkerServiceToken);
        return client;
    }
}
