namespace Baytology.Contracts.Requests.Recommendations;

public sealed record ResolveRecommendationResultRequest(
    Guid? RecommendedPropertyId,
    string? ExternalReference,
    float SimilarityScore,
    int Rank,
    string? SnapshotTitle,
    decimal? SnapshotPrice);
