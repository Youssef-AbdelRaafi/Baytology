namespace Baytology.Contracts.Responses.UserProfiles;

public sealed record UserProfileResponse(
    Guid Id,
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string? PhoneNumber,
    string PreferredContactMethod,
    DateTimeOffset CreatedOnUtc);
