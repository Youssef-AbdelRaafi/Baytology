using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Commands.AssignRole;

public class AssignRoleCommandHandler(IIdentityService identityService)
    : IRequestHandler<AssignRoleCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(AssignRoleCommand request, CancellationToken ct)
    {
        return await identityService.AssignRoleAsync(request.TargetUserId, request.Role);
    }
}
