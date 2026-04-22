using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Common.Models;
using Baytology.Application.Features.Admin.Commands.AssignRole;
using Baytology.Application.Features.Admin.Commands.ReviewRefund;
using Baytology.Application.Features.Admin.Commands.ToggleUserStatus;
using Baytology.Application.Features.Admin.Commands.VerifyAgent;
using Baytology.Application.Features.Admin.Queries.GetAgents;
using Baytology.Application.Features.Admin.Queries.GetAuditLogs;
using Baytology.Application.Features.Admin.Queries.GetDomainEventLogs;
using Baytology.Application.Features.Admin.Queries.GetPayments;
using Baytology.Application.Features.Admin.Queries.GetRecommendationRequests;
using Baytology.Application.Features.Admin.Queries.GetRefundRequests;
using Baytology.Application.Features.Admin.Queries.GetSearchRequests;
using Baytology.Application.Features.Admin.Queries.GetUsers;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Contracts.Common;
using Baytology.Contracts.Requests.Admin;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize(Roles = "Admin")]
public class AdminController(ISender sender) : ApiController
{
    [HttpGet("users")]
    [EndpointSummary("Get all users")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves all users with roles and active status for administration.")]
    [EndpointName("GetUsers")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var result = await sender.Send(new GetUsersQuery(), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("agents")]
    [EndpointSummary("Get all agents")]
    [ProducesResponseType(typeof(List<AdminAgentDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves all agents with profile, verification, and commission details.")]
    [EndpointName("GetAgents")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAgents(CancellationToken ct)
    {
        var result = await sender.Send(new GetAgentsQuery(), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPatch("users/{userId}/status")]
    [EndpointSummary("Toggle user active status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Activates or deactivates a user account.")]
    [EndpointName("ToggleUserStatus")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ToggleUserStatus(string userId, [FromBody] ToggleUserStatusRequest request, CancellationToken ct)
        => await ExecuteToggleUserStatus(userId, request, ct);

    [HttpPost("users/{userId}/toggle-status")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ToggleUserStatusLegacy(string userId, [FromBody] ToggleUserStatusRequest request, CancellationToken ct)
        => await ExecuteToggleUserStatus(userId, request, ct);

    private async Task<IActionResult> ExecuteToggleUserStatus(string userId, ToggleUserStatusRequest request, CancellationToken ct)
    {
        var command = new ToggleUserStatusCommand(userId, request.IsActive);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPatch("users/{userId}/role")]
    [EndpointSummary("Assign role to user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Assigns a new role (Buyer, Agent, Admin) to a user.")]
    [EndpointName("AssignRole")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request, CancellationToken ct)
        => await ExecuteAssignRole(userId, request, ct);

    [HttpPost("users/{userId}/assign-role")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> AssignRoleLegacy(string userId, [FromBody] AssignRoleRequest request, CancellationToken ct)
        => await ExecuteAssignRole(userId, request, ct);

    private async Task<IActionResult> ExecuteAssignRole(string userId, AssignRoleRequest request, CancellationToken ct)
    {
        var command = new AssignRoleCommand(userId, request.Role);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPatch("agents/{agentUserId}/verification")]
    [EndpointSummary("Verify an agent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Verifies an agent's profile.")]
    [EndpointName("VerifyAgent")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> VerifyAgent(string agentUserId, CancellationToken ct)
        => await ExecuteVerifyAgent(agentUserId, ct);

    [HttpPost("agents/{agentUserId}/verify")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> VerifyAgentLegacy(string agentUserId, CancellationToken ct)
        => await ExecuteVerifyAgent(agentUserId, ct);

    private async Task<IActionResult> ExecuteVerifyAgent(string agentUserId, CancellationToken ct)
    {
        var command = new VerifyAgentCommand(agentUserId);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPatch("refunds/{refundId:guid}/status")]
    [EndpointSummary("Review a refund request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Approves or rejects a pending refund request.")]
    [EndpointName("ReviewRefund")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ReviewRefund(Guid refundId, [FromBody] ReviewRefundRequest request, CancellationToken ct)
        => await ExecuteReviewRefund(refundId, request, ct);

    [HttpPost("refunds/{refundId:guid}/review")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [MapToApiVersion("1")]
    public async Task<IActionResult> ReviewRefundLegacy(Guid refundId, [FromBody] ReviewRefundRequest request, CancellationToken ct)
        => await ExecuteReviewRefund(refundId, request, ct);

    private async Task<IActionResult> ExecuteReviewRefund(Guid refundId, ReviewRefundRequest request, CancellationToken ct)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new ReviewRefundCommand(refundId, request.Approve, adminId);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpGet("audit-logs")]
    [EndpointSummary("Get system audit logs")]
    [ProducesResponseType(typeof(PaginatedList<AuditLogDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves a paginated list of system audit logs.")]
    [EndpointName("GetAuditLogs")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var query = new GetAuditLogsQuery(pageRequest.PageNumber, pageRequest.PageSize);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("payments")]
    [EndpointSummary("Get payments for admin monitoring")]
    [ProducesResponseType(typeof(PaginatedList<PaymentAdminDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves a paginated list of payments with their latest transaction state for admin review.")]
    [EndpointName("GetAdminPayments")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetPayments([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetPaymentsQuery(pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("refunds")]
    [EndpointSummary("Get refund requests for admin monitoring")]
    [ProducesResponseType(typeof(PaginatedList<RefundRequestAdminDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves a paginated list of refund requests and their review status.")]
    [EndpointName("GetRefundRequests")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetRefundRequests([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetRefundRequestsQuery(pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("search-requests")]
    [EndpointSummary("Get AI search requests for admin monitoring")]
    [ProducesResponseType(typeof(PaginatedList<SearchRequestAdminDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves AI search requests with queue/outbox visibility for operational monitoring.")]
    [EndpointName("GetAdminSearchRequests")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetSearchRequests([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetSearchRequestsQuery(pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("recommendation-requests")]
    [EndpointSummary("Get recommendation requests for admin monitoring")]
    [ProducesResponseType(typeof(PaginatedList<RecommendationRequestAdminDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves recommendation requests with queue/outbox visibility for operational monitoring.")]
    [EndpointName("GetAdminRecommendationRequests")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetRecommendationRequests([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetRecommendationRequestsQuery(pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("domain-events")]
    [EndpointSummary("Get outbox/domain event logs")]
    [ProducesResponseType(typeof(PaginatedList<DomainEventLogDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Retrieves a paginated list of domain event logs from the outbox for operational monitoring.")]
    [EndpointName("GetDomainEventLogs")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetDomainEventLogs([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetDomainEventLogsQuery(pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }
}
