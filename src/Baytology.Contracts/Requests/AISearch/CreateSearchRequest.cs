using Baytology.Contracts.Common;

namespace Baytology.Contracts.Requests.AISearch;

public sealed record CreateSearchRequest(
    SearchInputType InputType,
    SearchEngine SearchEngine,
    string? RawQuery,
    string? AudioFileUrl,
    string? ImageFileUrl,
    string? City,
    string? District,
    PropertyType? PropertyType,
    ListingType? ListingType,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MaxBedrooms);
