namespace Baytology.Contracts.Responses.Admin;

public sealed record UserSummaryResponse(
    string UserId,
    string Email,
    IReadOnlyCollection<string> Roles,
    bool IsActive,
    bool EmailConfirmed);
