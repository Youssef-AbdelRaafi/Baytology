namespace Baytology.Application.Common.Interfaces;

public interface IAiSearchFallbackService
{
    Task<AiSearchFallbackResolution?> ResolveAsync(Guid searchRequestId, CancellationToken ct = default);
}

public sealed record AiSearchFallbackResolution(
    bool IsSuccessful,
    IReadOnlyList<AiSearchFallbackResult> Results);

public sealed record AiSearchFallbackResult(
    Guid PropertyId,
    int Rank,
    float RelevanceScore,
    string ScoreSource,
    string? SnapshotTitle,
    decimal? SnapshotPrice,
    string? SnapshotCity,
    string? SnapshotStatus);

public interface IRecommendationFallbackService
{
    Task<RecommendationFallbackResolution?> ResolveAsync(Guid recommendationRequestId, CancellationToken ct = default);
}

public sealed record RecommendationFallbackResolution(
    bool IsSuccessful,
    IReadOnlyList<RecommendationFallbackResult> Results);

public sealed record RecommendationFallbackResult(
    Guid? RecommendedPropertyId,
    string? ExternalReference,
    float SimilarityScore,
    int Rank,
    string? SnapshotTitle,
    decimal? SnapshotPrice);
