using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.AgentDetails.Commands.UpdateAgentDetail;
using Baytology.Application.Features.AgentDetails.Dtos;
using Baytology.Application.Features.AgentDetails.Queries.GetAgentDetail;
using Baytology.Contracts.Requests.AgentDetails;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
public class AgentsController(ISender sender) : ApiController
{
    [HttpGet("{agentUserId}")]
    [EndpointSummary("Get agent public details")]
    [ProducesResponseType(typeof(AgentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns the public details for an agent, including rating and verification status.")]
    [EndpointName("GetAgentDetail")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAgentDetail(string agentUserId, CancellationToken ct)
    {
        var result = await sender.Send(new GetAgentDetailQuery(agentUserId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Agent,Admin")]
    [EndpointSummary("Get current agent details")]
    [ProducesResponseType(typeof(AgentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns the details for the current authenticated agent.")]
    [EndpointName("GetMyAgentDetail")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetMyAgentDetail(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetAgentDetailQuery(userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPut("me")]
    [Authorize(Roles = "Agent,Admin")]
    [EndpointSummary("Update the current agent details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Updates the authenticated agent's agency details and commission rate.")]
    [EndpointName("UpdateAgentDetail")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> UpdateMyAgentDetail([FromBody] UpdateAgentDetailRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new UpdateAgentDetailCommand(
            userId,
            request.AgencyName,
            request.LicenseNumber,
            request.CommissionRate);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(_ => Ok(), Problem);
    }
}
