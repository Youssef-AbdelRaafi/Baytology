using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetSavedProperties;

public record GetSavedPropertiesQuery(
    string UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PaginatedList<PropertyListItemDto>>>, ICachedQuery<Result<PaginatedList<PropertyListItemDto>>>
{
    public string CacheKey => ApplicationCacheKeys.SavedProperties(UserId, PageNumber, PageSize);

    public IEnumerable<string> Tags =>
    [
        ApplicationCacheTags.SavedProperties,
        ApplicationCacheTags.SavedPropertiesByUser(UserId),
        ApplicationCacheTags.Properties
    ];

    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);

    public TimeSpan? LocalCacheExpiration => TimeSpan.FromMinutes(1);
}
