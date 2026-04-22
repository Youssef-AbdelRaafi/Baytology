namespace Baytology.Application.Features.Identity.Dtos;

public sealed record UserSummaryDto(
    string UserId,
    string Email,
    IList<string> Roles,
    bool IsActive,
    bool EmailConfirmed);
