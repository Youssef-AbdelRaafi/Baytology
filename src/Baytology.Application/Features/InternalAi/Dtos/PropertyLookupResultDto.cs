namespace Baytology.Application.Features.InternalAi.Dtos;

public sealed record PropertyLookupItemDto(
    string? SourceListingUrl,
    string? Title,
    decimal? Price,
    string? City,
    string? District,
    string? PropertyType,
    decimal? Area,
    int? Bedrooms);

public sealed record PropertyLookupResultDto(
    int InputIndex,
    Guid? PropertyId,
    string? MatchSource,
    string? SourceListingUrl);
