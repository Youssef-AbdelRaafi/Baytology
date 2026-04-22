using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Recommendations;

public sealed class RecommendationResult : Entity
{
    public Guid RequestId { get; private set; }
    public Guid? RecommendedPropertyId { get; private set; }
    public string? ExternalReference { get; private set; }
    public float SimilarityScore { get; private set; }
    public int Rank { get; private set; }
    public string? SnapshotTitle { get; private set; }
    public decimal? SnapshotPrice { get; private set; }

    private RecommendationResult() { }

    private RecommendationResult(
        Guid requestId,
        Guid? recommendedPropertyId,
        string? externalReference,
        float similarityScore,
        int rank,
        string? snapshotTitle,
        decimal? snapshotPrice) : base(Guid.NewGuid())
    {
        RequestId = requestId;
        RecommendedPropertyId = recommendedPropertyId;
        ExternalReference = externalReference;
        SimilarityScore = similarityScore;
        Rank = rank;
        SnapshotTitle = snapshotTitle;
        SnapshotPrice = snapshotPrice;
    }

    public static Result<RecommendationResult> Create(
        Guid requestId,
        Guid? recommendedPropertyId,
        string? externalReference,
        float similarityScore,
        int rank,
        string? snapshotTitle,
        decimal? snapshotPrice)
    {
        if (requestId == Guid.Empty)
            return RecommendationErrors.RequestIdRequired;

        var normalizedExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (!recommendedPropertyId.HasValue && normalizedExternalReference is null)
            return RecommendationErrors.ReferenceRequired;

        if (rank <= 0)
            return RecommendationErrors.RankInvalid;

        if (similarityScore < 0)
            return RecommendationErrors.SimilarityScoreInvalid;

        if (normalizedExternalReference is not null && normalizedExternalReference.Length > 500)
            return RecommendationErrors.ExternalReferenceTooLong;

        var normalizedSnapshotTitle = string.IsNullOrWhiteSpace(snapshotTitle) ? null : snapshotTitle.Trim();
        if (normalizedSnapshotTitle is not null && normalizedSnapshotTitle.Length > 500)
            return RecommendationErrors.SnapshotTitleTooLong;

        if (snapshotPrice.HasValue && snapshotPrice.Value < 0)
            return RecommendationErrors.SnapshotPriceInvalid;

        return new RecommendationResult(
            requestId,
            recommendedPropertyId,
            normalizedExternalReference,
            similarityScore,
            rank,
            normalizedSnapshotTitle,
            snapshotPrice);
    }
}
