using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ChangePassword;

public class ChangePasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ChangePasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        return await identityService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword);
    }
}
