using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetAgents;

public class GetAgentsQueryHandler(IAppDbContext context, IIdentityService identityService)
    : IRequestHandler<GetAgentsQuery, Result<List<AdminAgentDto>>>
{
    public async Task<Result<List<AdminAgentDto>>> Handle(GetAgentsQuery request, CancellationToken ct)
    {
        var usersResult = await identityService.GetUsersAsync();

        if (usersResult.IsError)
            return usersResult.Errors;

        var usersById = usersResult.Value.ToDictionary(u => u.UserId, u => u);
        var activeAgentIds = usersResult.Value
            .Where(u => u.Roles.Any(role => string.Equals(role, "Agent", StringComparison.OrdinalIgnoreCase)))
            .Select(u => u.UserId)
            .ToArray();

        if (activeAgentIds.Length == 0)
            return new List<AdminAgentDto>();

        var agents = await context.AgentDetails
            .AsNoTracking()
            .Where(a => activeAgentIds.Contains(a.UserId))
            .OrderByDescending(a => a.IsVerified)
            .ThenByDescending(a => a.Rating)
            .Select(a => new
            {
                a.UserId,
                a.AgencyName,
                a.LicenseNumber,
                a.Rating,
                a.ReviewCount,
                a.IsVerified,
                a.CommissionRate,
                DisplayName = context.UserProfiles
                    .Where(p => p.UserId == a.UserId)
                    .Select(p => p.DisplayName)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return agents
            .Select(a => new AdminAgentDto(
                a.UserId,
                a.DisplayName,
                usersById.TryGetValue(a.UserId, out var user) ? user.Email : null,
                a.AgencyName,
                a.LicenseNumber,
                a.Rating,
                a.ReviewCount,
                a.IsVerified,
                a.CommissionRate))
            .ToList();
    }
}
