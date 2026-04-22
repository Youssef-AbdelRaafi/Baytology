namespace Baytology.Application.Features.UserProfiles.Dtos;

public record UserProfileDto(
    Guid Id,
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string? PhoneNumber,
    string PreferredContactMethod,
    DateTimeOffset CreatedOnUtc);
