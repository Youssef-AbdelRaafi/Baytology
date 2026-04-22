namespace Baytology.Application.Features.AgentDetails.Dtos;

public record AgentDetailDto(
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
