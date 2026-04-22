namespace Baytology.Contracts.Responses.Recommendations;

public sealed record RecommendationResultResponse(
    Guid? RecommendedPropertyId,
    string? ExternalReference,
    float SimilarityScore,
    int Rank,
    string? SnapshotTitle,
    decimal? SnapshotPrice);
