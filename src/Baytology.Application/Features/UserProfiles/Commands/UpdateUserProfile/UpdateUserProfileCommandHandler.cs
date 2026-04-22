using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateUserProfileCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var profile = await context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        if (profile is null)
            return Domain.UserProfiles.UserProfileErrors.NotFound;

        var updateResult = profile.Update(
            request.DisplayName,
            request.AvatarUrl,
            request.Bio,
            request.PhoneNumber,
            request.PreferredContactMethod);

        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }
}
