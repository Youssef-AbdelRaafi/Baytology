using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetProperties;

public record GetPropertiesQuery(
    string? City,
    string? District,
    string? PropertyType,
    string? ListingType,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinArea,
    decimal? MaxArea,
    int? MinBedrooms,
    int? MaxBedrooms,
    string? AgentUserId = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PaginatedList<PropertyListItemDto>>>, ICachedQuery<Result<PaginatedList<PropertyListItemDto>>>
{
    public string CacheKey => ApplicationCacheKeys.Properties(
        City,
        District,
        PropertyType,
        ListingType,
        MinPrice,
        MaxPrice,
        MinArea,
        MaxArea,
        MinBedrooms,
        MaxBedrooms,
        PageNumber,
        PageSize,
        AgentUserId);

    public IEnumerable<string> Tags => [ApplicationCacheTags.Properties];

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public TimeSpan? LocalCacheExpiration => TimeSpan.FromMinutes(2);
}
