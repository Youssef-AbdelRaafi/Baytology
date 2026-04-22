namespace Baytology.Application.Features.Recommendations.Dtos;

public record RecommendationRequestDto(
    Guid Id,
    string RequestedByUserId,
    string SourceEntityType,
    string? SourceEntityId,
    int TopN,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ResolvedAt,
    List<RecommendationResultDto> Results);
