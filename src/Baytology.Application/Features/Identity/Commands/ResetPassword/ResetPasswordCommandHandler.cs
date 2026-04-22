using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        return await identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
    }
}
