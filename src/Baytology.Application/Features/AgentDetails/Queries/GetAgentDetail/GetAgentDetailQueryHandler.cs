using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.AgentDetails.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AgentDetails.Queries.GetAgentDetail;

public class GetAgentDetailQueryHandler(IAppDbContext context, IIdentityService identityService)
    : IRequestHandler<GetAgentDetailQuery, Result<AgentDetailDto>>
{
    public async Task<Result<AgentDetailDto>> Handle(GetAgentDetailQuery request, CancellationToken ct)
    {
        if (!await identityService.IsInRoleAsync(request.UserId, "Agent"))
            return Domain.AgentDetails.AgentDetailErrors.NotFound;

        var agent = await context.AgentDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, ct);

        if (agent is null)
            return Domain.AgentDetails.AgentDetailErrors.NotFound;

        var profile = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        return new AgentDetailDto(
            agent.Id,
            agent.UserId,
            profile?.DisplayName,
            profile?.AvatarUrl,
            agent.AgencyName,
            agent.LicenseNumber,
            agent.Rating,
            agent.ReviewCount,
            agent.IsVerified,
            agent.CommissionRate,
            agent.CreatedOnUtc,
            agent.UpdatedOnUtc);
    }
}
