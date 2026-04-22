using System.Net;
using System.Net.Http.Json;

using Baytology.Api.Tests.Infrastructure;

namespace Baytology.Api.Tests.EndpointFlows;

public sealed class ExternalAiEndpointTests(ApiTestWebApplicationFactory factory)
    : IClassFixture<ApiTestWebApplicationFactory>
{
    [Fact]
    public async Task External_ai_proxy_endpoints_return_stubbed_chatbot_and_recommendation_payloads()
    {
        await factory.ResetDatabaseAsync();

        using var buyerClient = factory.CreateAuthenticatedClient(TestSeedData.BuyerUserId, TestSeedData.BuyerEmail, "Buyer");

        var statusResponse = await buyerClient.GetAsync("/api/v1/AiAssistant/status");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var statusPayload = await statusResponse.ReadJsonAsync();
        Assert.Equal("Online", statusPayload.GetProperty("overallStatus").GetString());

        var chatResponse = await buyerClient.PostAsJsonAsync("/api/v1/AiAssistant/chat", new
        {
            session_id = "session-1",
            message = "عايز شقة في القاهرة"
        });
        Assert.Equal(HttpStatusCode.OK, chatResponse.StatusCode);
        var chatPayload = await chatResponse.ReadJsonAsync();
        Assert.Equal("results", chatPayload.GetProperty("type").GetString());
        Assert.Equal(1, chatPayload.GetProperty("properties_count").GetInt32());

        var searchResponse = await buyerClient.PostAsJsonAsync("/api/v1/AiAssistant/search", new
        {
            filters = new
            {
                city = "Cairo"
            }
        });
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchPayload = await searchResponse.ReadJsonAsync();
        Assert.Equal(1, searchPayload.GetProperty("count").GetInt32());

        var recommendationResponse = await buyerClient.GetAsync("/api/v1/AiAssistant/recommend/500?topN=3");
        Assert.Equal(HttpStatusCode.OK, recommendationResponse.StatusCode);
        var recommendationPayload = await recommendationResponse.ReadJsonAsync();
        Assert.Equal(500, recommendationPayload.GetProperty("metadata").GetProperty("query_property_id").GetInt32());
    }
}
