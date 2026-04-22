namespace Baytology.Contracts.Responses.Properties;

public sealed record AgentSummaryResponse(
    string UserId,
    string? DisplayName,
    string? AvatarUrl,
    string? AgencyName,
    decimal Rating,
    int ReviewCount,
    bool IsVerified);
