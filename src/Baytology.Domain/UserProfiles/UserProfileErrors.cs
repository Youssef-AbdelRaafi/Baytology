using Baytology.Domain.Common.Results;

namespace Baytology.Domain.UserProfiles;

public static class UserProfileErrors
{
    public static readonly Error UserIdRequired =
        Error.Validation("UserProfile_UserId_Required", "User ID is required.");

    public static readonly Error DisplayNameRequired =
        Error.Validation("UserProfile_DisplayName_Required", "Display name is required.");

    public static readonly Error AvatarUrlTooLong =
        Error.Validation("UserProfile_AvatarUrl_TooLong", "Avatar URL cannot exceed 500 characters.");

    public static readonly Error BioTooLong =
        Error.Validation("UserProfile_Bio_TooLong", "Bio cannot exceed 2000 characters.");

    public static readonly Error PhoneNumberTooLong =
        Error.Validation("UserProfile_PhoneNumber_TooLong", "Phone number cannot exceed 20 characters.");

    public static readonly Error AlreadyExists =
        Error.Conflict("UserProfile_Already_Exists", "A profile already exists for this user.");

    public static readonly Error NotFound =
        Error.NotFound("UserProfile_Not_Found", "User profile not found.");
}
