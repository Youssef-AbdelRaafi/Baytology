using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.UserProfiles.Dtos;
using Baytology.Domain.Common.Results;
using Baytology.Domain.UserProfiles;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.UserProfiles.Queries.GetUserProfile;

public class GetUserProfileQueryHandler(IAppDbContext context)
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var profile = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        if (profile is null)
            return UserProfileErrors.NotFound;

        return new UserProfileDto(
            profile.Id,
            profile.UserId,
            profile.DisplayName,
            profile.AvatarUrl,
            profile.Bio,
            profile.PhoneNumber,
            profile.PreferredContactMethod.ToString(),
            profile.CreatedOnUtc);
    }
}
