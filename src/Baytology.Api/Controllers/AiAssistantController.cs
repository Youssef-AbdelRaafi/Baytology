using System.Text.Json;
using System.Text.Json.Nodes;

using Asp.Versioning;

using Baytology.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public sealed class AiAssistantController(
    IChatbotApiClient chatbotApiClient,
    IRecommendationApiClient recommendationApiClient) : ApiController
{
    [HttpGet("status")]
    [EndpointSummary("Get external AI integrations status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var chatbotStatus = await chatbotApiClient.GetStatusAsync(ct);
        var recommendationStatus = await recommendationApiClient.GetStatusAsync(ct);

        var payload = new JsonObject
        {
            ["overallStatus"] = chatbotStatus.IsSuccess && recommendationStatus.IsSuccess ? "Online" : "Degraded",
            ["chatbot"] = BuildStatusNode(chatbotStatus),
            ["recommendation"] = BuildStatusNode(recommendationStatus)
        };

        return Ok(payload);
    }

    [HttpPost("parse")]
    [EndpointSummary("Proxy parse request to the external chatbot API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Parse([FromBody] JsonElement request, CancellationToken ct)
    {
        var result = await chatbotApiClient.ParseAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("question")]
    [EndpointSummary("Proxy follow-up question request to the external chatbot API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Question([FromBody] JsonElement request, CancellationToken ct)
    {
        var result = await chatbotApiClient.AskQuestionAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("search")]
    [EndpointSummary("Proxy property search request to the external chatbot API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Search([FromBody] JsonElement request, CancellationToken ct)
    {
        var result = await chatbotApiClient.SearchAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("rank")]
    [EndpointSummary("Proxy ranking request to the external chatbot API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Rank([FromBody] JsonElement request, CancellationToken ct)
    {
        var result = await chatbotApiClient.RankAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("chat")]
    [EndpointSummary("Proxy chat request to the external chatbot API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Chat([FromBody] JsonElement request, CancellationToken ct)
    {
        var result = await chatbotApiClient.ChatAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("recommend/{houseId:int}")]
    [EndpointSummary("Proxy recommendation request to the external recommendation API")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> Recommend(int houseId, [FromQuery] int topN = 5, CancellationToken ct = default)
    {
        var result = await recommendationApiClient.RecommendAsync(houseId, topN, ct);
        return result.Match(Ok, Problem);
    }

    private static JsonNode BuildStatusNode(Baytology.Domain.Common.Results.Result<JsonNode?> result)
    {
        if (result.IsSuccess)
        {
            return result.Value?.DeepClone() ?? new JsonObject
            {
                ["available"] = true
            };
        }

        return new JsonObject
        {
            ["available"] = false,
            ["errorCode"] = result.TopError.Code,
            ["error"] = result.TopError.Description
        };
    }
}
