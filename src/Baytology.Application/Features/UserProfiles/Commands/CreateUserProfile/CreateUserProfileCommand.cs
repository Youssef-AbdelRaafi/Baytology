using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Features.UserProfiles.Commands.CreateUserProfile;

public record CreateUserProfileCommand(
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string? PhoneNumber,
    ContactMethod PreferredContactMethod = ContactMethod.Email) : IRequest<Result<Guid>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.UserProfiles,
        ApplicationCacheTags.UserProfile(UserId),
        ApplicationCacheTags.AgentDetails,
        ApplicationCacheTags.AgentDetail(UserId),
        ApplicationCacheTags.Properties
    ];
}
