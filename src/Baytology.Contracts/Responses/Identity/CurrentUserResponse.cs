namespace Baytology.Contracts.Responses.Identity;

public sealed record CurrentUserResponse(
    string UserId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<ClaimResponse> Claims,
    string? DisplayName = null);
