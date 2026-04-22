using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, Result<List<UserSummaryDto>>>
{
    public async Task<Result<List<UserSummaryDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        return await identityService.GetUsersAsync();
    }
}
