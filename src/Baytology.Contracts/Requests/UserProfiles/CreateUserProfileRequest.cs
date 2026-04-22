using Baytology.Contracts.Common;

namespace Baytology.Contracts.Requests.UserProfiles;

public sealed record CreateUserProfileRequest(
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string? PhoneNumber,
    ContactMethod PreferredContactMethod = ContactMethod.Email);
