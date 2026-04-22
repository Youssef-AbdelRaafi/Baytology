using Asp.Versioning;

using Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;
using Baytology.Application.Features.Recommendations.Commands.CreateRecommendationRequest;
using Baytology.Application.Features.Recommendations.Dtos;
using Baytology.Application.Features.Recommendations.Queries.GetRecommendationRequest;
using Baytology.Contracts.Requests.Recommendations;
using Baytology.Contracts.Responses.Recommendations;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public class RecommendationsController(ISender sender) : ApiController
{
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get AI recommendation request status and results")]
    [ProducesResponseType(typeof(RecommendationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns the status and results of an AI recommendation request.")]
    [EndpointName("GetRecommendationRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetRecommendation(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetRecommendationRequestQuery(id, userId);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EndpointSummary("Request AI recommendations")]
    [ProducesResponseType(typeof(CreateRecommendationRequestResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Requests AI recommendations for a given source entity. Returns the created recommendation request id.")]
    [EndpointName("CreateRecommendationRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateRecommendation([FromBody] CreateRecommendationRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreateRecommendationRequestCommand(userId, request.SourceEntityType, request.SourceEntityId, request.TopN);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(
            id => AcceptedAtAction(nameof(GetRecommendation), new { id }, new CreateRecommendationRequestResponse(id)),
            Problem);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Resolve a recommendation request with worker results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Allows the system/admin worker pipeline to persist recommendation results with snapshots.")]
    [EndpointName("ResolveRecommendationRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ResolveRecommendation(Guid id, [FromBody] ResolveRecommendationRequest request, CancellationToken ct)
    {
        var command = new CompleteRecommendationRequestCommand(
            id,
            request.IsSuccessful,
            request.Results?
                .Select(result => new CompleteRecommendationResultInput(
                    result.RecommendedPropertyId,
                    result.ExternalReference,
                    result.SimilarityScore,
                    result.Rank,
                    result.SnapshotTitle,
                    result.SnapshotPrice))
                .ToList());

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }
}
