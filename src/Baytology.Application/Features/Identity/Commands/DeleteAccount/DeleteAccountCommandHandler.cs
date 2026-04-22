using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.DeleteAccount;

public class DeleteAccountCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteAccountCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeleteAccountCommand request, CancellationToken ct)
    {
        return await identityService.DeleteAccountAsync(request.UserId);
    }
}
