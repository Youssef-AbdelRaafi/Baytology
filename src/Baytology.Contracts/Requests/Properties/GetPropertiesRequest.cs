using Baytology.Contracts.Common;

namespace Baytology.Contracts.Requests.Properties;

public sealed record GetPropertiesRequest(
    string? City,
    string? District,
    PropertyType? PropertyType,
    ListingType? ListingType,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MaxBedrooms,
    string? AgentUserId,
    int PageNumber = 1,
    int PageSize = 10);
