namespace Baytology.Contracts.Requests.AISearch;

public sealed record ResolveSearchResultRequest(
    Guid PropertyId,
    int Rank,
    float RelevanceScore,
    string? ScoreSource,
    string? SnapshotTitle,
    decimal? SnapshotPrice,
    string? SnapshotCity,
    string? SnapshotStatus);
