using System.Text.Json;
using System.Text.Json.Nodes;

using Baytology.Domain.Common.Results;

namespace Baytology.Application.Common.Interfaces;

public interface IChatbotApiClient
{
    Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default);
    Task<Result<JsonNode?>> ParseAsync(JsonElement payload, CancellationToken ct = default);
    Task<Result<JsonNode?>> AskQuestionAsync(JsonElement payload, CancellationToken ct = default);
    Task<Result<JsonNode?>> SearchAsync(JsonElement payload, CancellationToken ct = default);
    Task<Result<JsonNode?>> RankAsync(JsonElement payload, CancellationToken ct = default);
    Task<Result<JsonNode?>> ChatAsync(JsonElement payload, CancellationToken ct = default);
}

public interface IRecommendationApiClient
{
    Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default);
    Task<Result<JsonNode?>> RecommendAsync(int houseId, int topN, CancellationToken ct = default);
}
