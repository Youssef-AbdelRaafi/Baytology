using Asp.Versioning;

using Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;
using Baytology.Application.Features.AISearch.Commands.CreateSearchRequest;
using Baytology.Application.Features.AISearch.Dtos;
using Baytology.Application.Features.AISearch.Queries.GetSearchRequest;
using Baytology.Contracts.Requests.AISearch;
using Baytology.Contracts.Responses.AISearch;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public class SearchController(ISender sender) : ApiController
{
    [HttpGet("{id:guid}")]
    [EndpointSummary("Get AI search request status and results")]
    [ProducesResponseType(typeof(SearchRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns the status and results of an AI search request.")]
    [EndpointName("GetAISearchRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetSearch(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetSearchRequestQuery(id, userId);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EndpointSummary("Create a new AI search request")]
    [ProducesResponseType(typeof(CreateSearchRequestResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates an AI search request and publishes it to RabbitMQ for AI processing. Returns the search request id.")]
    [EndpointName("CreateAISearchRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateSearch([FromBody] CreateSearchRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreateSearchRequestCommand(
            userId,
            (Baytology.Domain.Common.Enums.SearchInputType)request.InputType,
            (Baytology.Domain.Common.Enums.SearchEngine)request.SearchEngine,
            request.RawQuery,
            request.AudioFileUrl,
            request.ImageFileUrl,
            request.City,
            request.District,
            request.PropertyType?.ToString(),
            request.ListingType?.ToString(),
            request.MinPrice,
            request.MaxPrice,
            request.MinArea,
            request.MaxArea,
            request.MinBedrooms,
            request.MaxBedrooms);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(
            id => AcceptedAtAction(nameof(GetSearch), new { id }, new CreateSearchRequestResponse(id)),
            Problem);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Resolve an AI search request with worker results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Allows the system/admin worker pipeline to persist ranked AI search results with snapshots.")]
    [EndpointName("ResolveAISearchRequest")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ResolveSearch(Guid id, [FromBody] ResolveSearchRequest request, CancellationToken ct)
    {
        var command = new CompleteSearchRequestCommand(
            id,
            request.IsSuccessful,
            request.Results?
                .Select(result => new CompleteSearchResultInput(
                    result.PropertyId,
                    result.Rank,
                    result.RelevanceScore,
                    result.ScoreSource,
                    result.SnapshotTitle,
                    result.SnapshotPrice,
                    result.SnapshotCity,
                    result.SnapshotStatus))
                .ToList());

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }
}
