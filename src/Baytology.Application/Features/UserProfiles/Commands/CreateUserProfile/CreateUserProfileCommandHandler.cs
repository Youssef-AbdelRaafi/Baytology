using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.UserProfiles;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.UserProfiles.Commands.CreateUserProfile;

public class CreateUserProfileCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateUserProfileCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserProfileCommand request, CancellationToken ct)
    {
        var exists = await context.UserProfiles.AnyAsync(p => p.UserId == request.UserId, ct);
        if (exists)
            return UserProfileErrors.AlreadyExists;

        var profileResult = UserProfile.Create(
            request.UserId,
            request.DisplayName,
            request.AvatarUrl,
            request.Bio,
            request.PhoneNumber,
            request.PreferredContactMethod);

        if (profileResult.IsError)
            return profileResult.Errors;

        context.UserProfiles.Add(profileResult.Value);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var duplicateExists = await context.UserProfiles
                .AnyAsync(p => p.UserId == request.UserId, ct);

            if (duplicateExists)
                return UserProfileErrors.AlreadyExists;

            throw;
        }

        return profileResult.Value.Id;
    }
}
