using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public sealed class SearchResult : Entity
{
    public Guid SearchRequestId { get; private set; }
    public Guid PropertyId { get; private set; }
    public int Rank { get; private set; }
    public float RelevanceScore { get; private set; }
    public string? ScoreSource { get; private set; }
    public string? SnapshotTitle { get; private set; }
    public decimal? SnapshotPrice { get; private set; }
    public string? SnapshotCity { get; private set; }
    public string? SnapshotStatus { get; private set; }

    private SearchResult() { }

    private SearchResult(
        Guid searchRequestId,
        Guid propertyId,
        int rank,
        float relevanceScore,
        string? scoreSource,
        string? snapshotTitle,
        decimal? snapshotPrice,
        string? snapshotCity,
        string? snapshotStatus) : base(Guid.NewGuid())
    {
        SearchRequestId = searchRequestId;
        PropertyId = propertyId;
        Rank = rank;
        RelevanceScore = relevanceScore;
        ScoreSource = scoreSource;
        SnapshotTitle = snapshotTitle;
        SnapshotPrice = snapshotPrice;
        SnapshotCity = snapshotCity;
        SnapshotStatus = snapshotStatus;
    }

    public static Result<SearchResult> Create(
        Guid searchRequestId,
        Guid propertyId,
        int rank,
        float relevanceScore,
        string? scoreSource,
        string? snapshotTitle,
        decimal? snapshotPrice,
        string? snapshotCity,
        string? snapshotStatus)
    {
        if (searchRequestId == Guid.Empty)
            return SearchErrors.SearchRequestIdRequired;

        if (propertyId == Guid.Empty)
            return SearchErrors.PropertyIdRequired;

        if (rank <= 0)
            return SearchErrors.RankInvalid;

        if (relevanceScore < 0)
            return SearchErrors.RelevanceScoreInvalid;

        var normalizedScoreSource = string.IsNullOrWhiteSpace(scoreSource) ? null : scoreSource.Trim();
        if (normalizedScoreSource is not null && normalizedScoreSource.Length > 50)
            return SearchErrors.ScoreSourceTooLong;

        var normalizedSnapshotTitle = string.IsNullOrWhiteSpace(snapshotTitle) ? null : snapshotTitle.Trim();
        if (normalizedSnapshotTitle is not null && normalizedSnapshotTitle.Length > 500)
            return SearchErrors.SnapshotTitleTooLong;

        if (snapshotPrice.HasValue && snapshotPrice.Value < 0)
            return SearchErrors.SnapshotPriceInvalid;

        var normalizedSnapshotCity = string.IsNullOrWhiteSpace(snapshotCity) ? null : snapshotCity.Trim();
        if (normalizedSnapshotCity is not null && normalizedSnapshotCity.Length > 100)
            return SearchErrors.SnapshotCityTooLong;

        var normalizedSnapshotStatus = string.IsNullOrWhiteSpace(snapshotStatus) ? null : snapshotStatus.Trim();
        if (normalizedSnapshotStatus is not null && normalizedSnapshotStatus.Length > 30)
            return SearchErrors.SnapshotStatusTooLong;

        return new SearchResult(
            searchRequestId,
            propertyId,
            rank,
            relevanceScore,
            normalizedScoreSource,
            normalizedSnapshotTitle,
            snapshotPrice,
            normalizedSnapshotCity,
            normalizedSnapshotStatus);
    }
}
