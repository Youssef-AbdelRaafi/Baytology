using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using MediatR;

namespace Baytology.Application.Features.Identity.Commands.Logout;

public class LogoutCommandHandler(IIdentityService identityService)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand request, CancellationToken ct)
    {
        return await identityService.RevokeRefreshTokensAsync(request.UserId);
    }
}
