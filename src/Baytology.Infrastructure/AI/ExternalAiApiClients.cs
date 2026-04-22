using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.AI;

internal sealed class ChatbotApiClient(
    HttpClient httpClient,
    IOptions<ExternalAiServicesSettings> settings,
    ILogger<ChatbotApiClient> logger)
    : IChatbotApiClient
{
    private readonly ExternalAiServicesSettings _settings = settings.Value;

    public Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default)
        => GetAsync("/", "Chatbot", isEnabled: _settings.ChatbotEnabled, ct);

    public Task<Result<JsonNode?>> ParseAsync(JsonElement payload, CancellationToken ct = default)
        => PostAsync("/parse", payload, "Chatbot.Parse", isEnabled: _settings.ChatbotEnabled, ct);

    public Task<Result<JsonNode?>> AskQuestionAsync(JsonElement payload, CancellationToken ct = default)
        => PostAsync("/question", payload, "Chatbot.Question", isEnabled: _settings.ChatbotEnabled, ct);

    public Task<Result<JsonNode?>> SearchAsync(JsonElement payload, CancellationToken ct = default)
        => PostAsync("/search", payload, "Chatbot.Search", isEnabled: _settings.ChatbotEnabled, ct);

    public Task<Result<JsonNode?>> RankAsync(JsonElement payload, CancellationToken ct = default)
        => PostAsync("/rank", payload, "Chatbot.Rank", isEnabled: _settings.ChatbotEnabled, ct);

    public Task<Result<JsonNode?>> ChatAsync(JsonElement payload, CancellationToken ct = default)
        => PostAsync("/chat", payload, "Chatbot.Chat", isEnabled: _settings.ChatbotEnabled, ct);

    private async Task<Result<JsonNode?>> GetAsync(string path, string serviceName, bool isEnabled, CancellationToken ct)
    {
        if (!TryValidateConfiguration(serviceName, isEnabled, _settings.ChatbotBaseUrl, out var error))
            return error;

        try
        {
            using var response = await httpClient.GetAsync(path, ct);
            return await ReadResponseAsync(response, serviceName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ServiceName} request failed.", serviceName);
            return ApplicationErrors.ExternalAi.Unavailable(serviceName);
        }
    }

    private async Task<Result<JsonNode?>> PostAsync(
        string path,
        JsonElement payload,
        string serviceName,
        bool isEnabled,
        CancellationToken ct)
    {
        if (!TryValidateConfiguration(serviceName, isEnabled, _settings.ChatbotBaseUrl, out var error))
            return error;

        try
        {
            using var response = await httpClient.PostAsJsonAsync(path, payload, cancellationToken: ct);
            return await ReadResponseAsync(response, serviceName, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ServiceName} request failed.", serviceName);
            return ApplicationErrors.ExternalAi.Unavailable(serviceName);
        }
    }

    private static bool TryValidateConfiguration(string serviceName, bool isEnabled, string baseUrl, out Error error)
    {
        if (!isEnabled)
        {
            error = ApplicationErrors.ExternalAi.Disabled(serviceName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            error = ApplicationErrors.ExternalAi.NotConfigured(serviceName);
            return false;
        }

        error = default;
        return true;
    }

    private static async Task<Result<JsonNode?>> ReadResponseAsync(HttpResponseMessage response, string serviceName, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return ApplicationErrors.ExternalAi.Failed(serviceName, (int)response.StatusCode, body);
        }

        if (string.IsNullOrWhiteSpace(body))
            return (JsonNode?)null;

        return JsonNode.Parse(body);
    }

    private static string Truncate(string value)
    {
        return value.Length <= 300 ? value : value[..300];
    }
}

internal sealed class RecommendationApiClient(
    HttpClient httpClient,
    IOptions<ExternalAiServicesSettings> settings,
    ILogger<RecommendationApiClient> logger)
    : IRecommendationApiClient
{
    private readonly ExternalAiServicesSettings _settings = settings.Value;

    public Task<Result<JsonNode?>> GetStatusAsync(CancellationToken ct = default)
        => GetAsync("/", "Recommendation", ct);

    public Task<Result<JsonNode?>> RecommendAsync(int houseId, int topN, CancellationToken ct = default)
    {
        var safeTopN = Math.Clamp(topN, 1, 20);
        return GetAsync($"/recommend/{houseId}?n={safeTopN}", "Recommendation", ct);
    }

    private async Task<Result<JsonNode?>> GetAsync(string path, string serviceName, CancellationToken ct)
    {
        if (!TryValidateConfiguration(serviceName, _settings.RecommendationEnabled, _settings.RecommendationBaseUrl, out var error))
            return error;

        try
        {
            using var response = await httpClient.GetAsync(path, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
                return ApplicationErrors.ExternalAi.Failed(serviceName, (int)response.StatusCode, body);
        }

            if (string.IsNullOrWhiteSpace(body))
                return (JsonNode?)null;

            return JsonNode.Parse(body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ServiceName} request failed.", serviceName);
            return ApplicationErrors.ExternalAi.Unavailable(serviceName);
        }
    }

    private static bool TryValidateConfiguration(string serviceName, bool isEnabled, string baseUrl, out Error error)
    {
        if (!isEnabled)
        {
            error = ApplicationErrors.ExternalAi.Disabled(serviceName);
            return false;
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            error = ApplicationErrors.ExternalAi.NotConfigured(serviceName);
            return false;
        }

        error = default;
        return true;
    }
}
