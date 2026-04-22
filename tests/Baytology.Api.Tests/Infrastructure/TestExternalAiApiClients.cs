using System.Text.Json;
using System.Text.Json.Nodes;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

namespace Baytology.Api.Tests.Infrastructure;

internal sealed class TestChatbotApiClient : IChatbotApiClient
{
    public Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"status":"ok","message":"chatbot online"}"""));

    public Task<Result<JsonNode?>> ParseAsync(JsonElement payload, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"filters":{"city":"Cairo"},"message":"Parsed successfully"}"""));

    public Task<Result<JsonNode?>> AskQuestionAsync(JsonElement payload, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"question":"عايز كام أوضة؟","attribute":"min_bedrooms","has_question":true}"""));

    public Task<Result<JsonNode?>> SearchAsync(JsonElement payload, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"count":1,"properties":[{"title":"Proxy Search Result","city":"Cairo"}]}"""));

    public Task<Result<JsonNode?>> RankAsync(JsonElement payload, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"ranked":[{"title":"Ranked Result","score":0.91}]}"""));

    public Task<Result<JsonNode?>> ChatAsync(JsonElement payload, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"type":"results","message":"Proxy chat response","properties_count":1,"properties":[{"title":"Chat Result"}]}"""));
}

internal sealed class TestRecommendationApiClient : IRecommendationApiClient
{
    public Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse("""{"status":"Online","engine":"FAISS"}"""));

    public Task<Result<JsonNode?>> RecommendAsync(int houseId, int topN, CancellationToken ct = default)
        => Task.FromResult<Result<JsonNode?>>(JsonNode.Parse(
            $$"""{"metadata":{"query_property_id":{{houseId}},"total_recommendations_found":1},"best_match":{"id":101},"all_recommendations":[{"id":101,"similarity_score":0.0123}]}"""));
}
