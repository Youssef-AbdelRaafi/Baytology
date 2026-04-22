namespace Baytology.Contracts.Responses.Admin;

public sealed record AdminAgentResponse(
    string UserId,
    string? DisplayName,
    string? Email,
    string? AgencyName,
    string? LicenseNumber,
    decimal Rating,
    int ReviewCount,
    bool IsVerified,
    decimal CommissionRate);
