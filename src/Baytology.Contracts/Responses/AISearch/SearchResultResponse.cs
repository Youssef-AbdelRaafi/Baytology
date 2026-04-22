namespace Baytology.Contracts.Responses.AISearch;

public sealed record SearchResultResponse(
    Guid PropertyId,
    int Rank,
    float RelevanceScore,
    string? ScoreSource,
    string? SnapshotTitle,
    decimal? SnapshotPrice,
    string? SnapshotCity,
    string? SnapshotStatus);
