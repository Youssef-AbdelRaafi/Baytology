namespace Baytology.Contracts.Responses.Properties;

public sealed record PropertyImageResponse(
    Guid Id,
    string Url,
    bool IsPrimary,
    int SortOrder);
