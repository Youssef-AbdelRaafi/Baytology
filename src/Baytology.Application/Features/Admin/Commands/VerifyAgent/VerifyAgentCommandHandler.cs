using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Commands.VerifyAgent;

public class VerifyAgentCommandHandler(IAppDbContext context, IIdentityService identityService)
    : IRequestHandler<VerifyAgentCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(VerifyAgentCommand request, CancellationToken ct)
    {
        if (!await identityService.IsInRoleAsync(request.AgentUserId, "Agent"))
            return ApplicationErrors.Admin.AgentNotFound;

        var agent = await context.AgentDetails.FirstOrDefaultAsync(a => a.UserId == request.AgentUserId, ct);
        if (agent is null) return ApplicationErrors.Admin.AgentNotFound;

        agent.Verify();
        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
