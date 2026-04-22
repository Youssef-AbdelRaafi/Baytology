using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler(IIdentityService identityService)
    : IRequestHandler<ConfirmEmailCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        return await identityService.ConfirmEmailAsync(request.UserId, request.Token);
    }
}
