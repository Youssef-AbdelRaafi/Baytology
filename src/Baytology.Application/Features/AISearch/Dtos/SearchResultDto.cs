namespace Baytology.Application.Features.AISearch.Dtos;

public record SearchResultDto(
    Guid PropertyId,
    int Rank,
    float RelevanceScore,
    string? ScoreSource,
    string? SnapshotTitle,
    decimal? SnapshotPrice,
    string? SnapshotCity,
    string? SnapshotStatus);
