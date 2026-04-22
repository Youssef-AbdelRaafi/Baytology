namespace Baytology.Application.Features.Recommendations.Dtos;

public record RecommendationResultDto(
    Guid? RecommendedPropertyId,
    string? ExternalReference,
    float SimilarityScore,
    int Rank,
    string? SnapshotTitle,
    decimal? SnapshotPrice);
