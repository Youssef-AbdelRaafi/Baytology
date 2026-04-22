using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetAgents;

public record AdminAgentDto(
    string UserId,
    string? DisplayName,
    string? Email,
    string? AgencyName,
    string? LicenseNumber,
    decimal Rating,
    int ReviewCount,
    bool IsVerified,
    decimal CommissionRate);

public record GetAgentsQuery() : IRequest<Result<List<AdminAgentDto>>>;
