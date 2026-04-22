using System.Security.Cryptography;
using System.Text;

using Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;
using Baytology.Application.Features.InternalAi.Dtos;
using Baytology.Application.Features.InternalAi.Queries.LookupPropertyMappings;
using Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;
using Baytology.Contracts.Requests.AISearch;
using Baytology.Contracts.Requests.InternalAi;
using Baytology.Contracts.Requests.Recommendations;
using Baytology.Contracts.Responses.InternalAi;
using Baytology.Infrastructure.Settings;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Baytology.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/internal/ai")]
public sealed class InternalAiWorkerController(
    ISender sender,
    IOptions<AiWorkerSettings> workerOptions) : ControllerBase
{
    private readonly AiWorkerSettings _workerSettings = workerOptions.Value;

    [HttpPost("property-mappings/lookup")]
    public async Task<IActionResult> LookupPropertyMappings([FromBody] LookupPropertyMappingsRequest request, CancellationToken ct)
    {
        if (!IsAuthorized(Request))
            return Unauthorized();

        var query = new LookupPropertyMappingsQuery(
            request.Items.Select(item => new PropertyLookupItemDto(
                item.SourceListingUrl,
                item.Title,
                item.Price,
                item.City,
                item.District,
                item.PropertyType,
                item.Area,
                item.Bedrooms)).ToList());

        var result = await sender.Send(query, ct);

        if (result.IsError)
            return StatusCode(StatusCodes.Status500InternalServerError);

        return Ok(new LookupPropertyMappingsResponse(
            result.Value.Select(item => new PropertyLookupResultResponse(
                item.InputIndex,
                item.PropertyId,
                item.MatchSource,
                item.SourceListingUrl)).ToList()));
    }

    [HttpPost("search/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveSearch(Guid id, [FromBody] ResolveSearchRequest request, CancellationToken ct)
    {
        if (!IsAuthorized(Request))
            return Unauthorized();

        var command = new CompleteSearchRequestCommand(
            id,
            request.IsSuccessful,
            request.Results?
                .Select(item => new CompleteSearchResultInput(
                    item.PropertyId,
                    item.Rank,
                    item.RelevanceScore,
                    item.ScoreSource,
                    item.SnapshotTitle,
                    item.SnapshotPrice,
                    item.SnapshotCity,
                    item.SnapshotStatus))
                .ToList());

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok() : StatusCode(StatusCodes.Status409Conflict, result.Errors);
    }

    [HttpPost("recommendations/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveRecommendation(Guid id, [FromBody] ResolveRecommendationRequest request, CancellationToken ct)
    {
        if (!IsAuthorized(Request))
            return Unauthorized();

        var command = new CompleteRecommendationRequestCommand(
            id,
            request.IsSuccessful,
            request.Results?
                .Select(item => new CompleteRecommendationResultInput(
                    item.RecommendedPropertyId,
                    item.ExternalReference,
                    item.SimilarityScore,
                    item.Rank,
                    item.SnapshotTitle,
                    item.SnapshotPrice))
                .ToList());

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok() : StatusCode(StatusCodes.Status409Conflict, result.Errors);
    }

    private bool IsAuthorized(HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(_workerSettings.ServiceToken))
            return false;

        var headerName = string.IsNullOrWhiteSpace(_workerSettings.ServiceTokenHeaderName)
            ? "X-AI-Service-Token"
            : _workerSettings.ServiceTokenHeaderName;

        var providedToken = request.Headers[headerName].FirstOrDefault();
        return FixedTimeEquals(providedToken, _workerSettings.ServiceToken);
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        if (leftBytes.Length != rightBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
