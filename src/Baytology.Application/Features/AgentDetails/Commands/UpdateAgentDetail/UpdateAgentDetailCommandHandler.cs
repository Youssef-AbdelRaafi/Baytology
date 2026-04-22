using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AgentDetails.Commands.UpdateAgentDetail;

public class UpdateAgentDetailCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateAgentDetailCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateAgentDetailCommand request, CancellationToken ct)
    {
        var agent = await context.AgentDetails
            .FirstOrDefaultAsync(a => a.UserId == request.UserId, ct);

        if (agent is null)
            return ApplicationErrors.AgentDetails.NotFound;

        if (request.CommissionRate <= 0 || request.CommissionRate >= 1)
            return ApplicationErrors.AgentDetails.InvalidCommissionRate;

        agent.Update(request.AgencyName, request.LicenseNumber, request.CommissionRate);
        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
