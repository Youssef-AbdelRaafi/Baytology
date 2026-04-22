namespace Baytology.Application.Features.Properties.Dtos;

public record PropertyListItemDto(
    Guid Id,
    string AgentUserId,
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
