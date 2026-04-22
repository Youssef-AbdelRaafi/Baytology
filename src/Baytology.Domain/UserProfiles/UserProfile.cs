using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.UserProfiles;

public sealed class UserProfile : AuditableEntity
{
    public string UserId { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public string? PhoneNumber { get; private set; }
    public ContactMethod PreferredContactMethod { get; private set; }

    private UserProfile() { }

    private UserProfile(
        Guid id,
        string userId,
        string displayName,
        string? avatarUrl,
        string? bio,
        string? phoneNumber,
        ContactMethod preferredContactMethod) : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
        Bio = bio;
        PhoneNumber = phoneNumber;
        PreferredContactMethod = preferredContactMethod;
    }

    public static Result<UserProfile> Create(
        string userId,
        string displayName,
        string? avatarUrl = null,
        string? bio = null,
        string? phoneNumber = null,
        ContactMethod preferredContactMethod = ContactMethod.Email)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return UserProfileErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(displayName))
            return UserProfileErrors.DisplayNameRequired;

        if (!string.IsNullOrWhiteSpace(avatarUrl) && avatarUrl.Trim().Length > 500)
            return UserProfileErrors.AvatarUrlTooLong;

        if (!string.IsNullOrWhiteSpace(bio) && bio.Trim().Length > 2000)
            return UserProfileErrors.BioTooLong;

        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Trim().Length > 20)
            return UserProfileErrors.PhoneNumberTooLong;

        return new UserProfile(
            Guid.NewGuid(), userId, displayName,
            string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim(),
            string.IsNullOrWhiteSpace(bio) ? null : bio.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            preferredContactMethod);
    }

    public Result<Success> Update(
        string displayName,
        string? avatarUrl,
        string? bio,
        string? phoneNumber,
        ContactMethod preferredContactMethod)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return UserProfileErrors.DisplayNameRequired;

        if (!string.IsNullOrWhiteSpace(avatarUrl) && avatarUrl.Trim().Length > 500)
            return UserProfileErrors.AvatarUrlTooLong;

        if (!string.IsNullOrWhiteSpace(bio) && bio.Trim().Length > 2000)
            return UserProfileErrors.BioTooLong;

        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Trim().Length > 20)
            return UserProfileErrors.PhoneNumberTooLong;

        DisplayName = displayName;
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        PreferredContactMethod = preferredContactMethod;

        return Result.Success;
    }
}
