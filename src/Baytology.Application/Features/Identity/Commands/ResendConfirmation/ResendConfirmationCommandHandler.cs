using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ResendConfirmation;

public class ResendConfirmationCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResendConfirmationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResendConfirmationCommand request, CancellationToken ct)
    {
        return await identityService.ResendEmailConfirmationAsync(request.Email);
    }
}
