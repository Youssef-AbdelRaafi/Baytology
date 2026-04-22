namespace Baytology.Contracts.Requests.InternalAi;

public sealed record LookupPropertyMappingsRequest(
    List<PropertyLookupItemRequest> Items);

public sealed record PropertyLookupItemRequest(
    string? SourceListingUrl,
    string? Title,
    decimal? Price,
    string? City,
    string? District,
    string? PropertyType,
    decimal? Area,
    int? Bedrooms);
