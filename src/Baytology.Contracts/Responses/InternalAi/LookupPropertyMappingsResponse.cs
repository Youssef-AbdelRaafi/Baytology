namespace Baytology.Contracts.Responses.InternalAi;

public sealed record LookupPropertyMappingsResponse(
    List<PropertyLookupResultResponse> Results);

public sealed record PropertyLookupResultResponse(
    int InputIndex,
    Guid? PropertyId,
    string? MatchSource,
    string? SourceListingUrl);
