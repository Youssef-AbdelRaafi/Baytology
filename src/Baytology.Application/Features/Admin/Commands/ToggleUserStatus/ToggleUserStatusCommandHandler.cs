using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler(IIdentityService identityService)
    : IRequestHandler<ToggleUserStatusCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ToggleUserStatusCommand request, CancellationToken ct)
    {
        return await identityService.ToggleUserStatusAsync(request.TargetUserId, request.IsActive);
    }
}
