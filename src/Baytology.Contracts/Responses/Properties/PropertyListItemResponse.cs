namespace Baytology.Contracts.Responses.Properties;

public sealed record PropertyListItemResponse(
    Guid Id,
    string Title,
    decimal Price,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    string? City,
    string? District,
    string PropertyType,
    string ListingType,
    string Status,
    bool IsFeatured,
    string? PrimaryImageUrl);
