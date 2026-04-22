namespace Baytology.Contracts.Responses.AgentDetails;

public sealed record AgentDetailResponse(
    Guid Id,
    string UserId,
    string? DisplayName,
    string? AvatarUrl,
    string? AgencyName,
    string? LicenseNumber,
    decimal Rating,
    int ReviewCount,
    bool IsVerified,
    decimal CommissionRate,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset UpdatedOnUtc);
