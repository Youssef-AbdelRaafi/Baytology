using Baytology.Contracts.Common;

namespace Baytology.Contracts.Requests.Properties;

public sealed record CreatePropertyRequest(
    string Title,
    string? Description,
    PropertyType PropertyType,
    ListingType ListingType,
    decimal Price,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    int? Floor,
    int? TotalFloors,
    string? AddressLine,
    string? City,
    string? District,
    string? ZipCode,
    decimal? Latitude,
    decimal? Longitude,
    bool HasParking = false,
    bool HasPool = false,
    bool HasGym = false,
    bool HasElevator = false,
    bool HasSecurity = false,
    bool HasBalcony = false,
    bool HasGarden = false,
    bool HasCentralAC = false,
    FurnishingStatus FurnishingStatus = FurnishingStatus.Unfurnished,
    ViewType? ViewType = null,
    List<string>? ImageUrls = null);
