namespace Baytology.Application.Features.Properties.Dtos;

public record AgentSummaryDto(
    string UserId,
    string? DisplayName,
    string? AvatarUrl,
    string? AgencyName,
    decimal Rating,
    int ReviewCount,
    bool IsVerified);
