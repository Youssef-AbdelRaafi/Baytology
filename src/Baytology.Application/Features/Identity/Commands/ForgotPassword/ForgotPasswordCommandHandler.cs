using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ForgotPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        return await identityService.ForgotPasswordAsync(request.Email);
    }
}
