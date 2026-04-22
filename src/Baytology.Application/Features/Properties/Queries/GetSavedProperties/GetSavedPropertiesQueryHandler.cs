using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetSavedProperties;

public class GetSavedPropertiesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetSavedPropertiesQuery, Result<PaginatedList<PropertyListItemDto>>>
{
    public async Task<Result<PaginatedList<PropertyListItemDto>>> Handle(GetSavedPropertiesQuery request, CancellationToken ct)
    {
        var savedQuery = context.SavedProperties
            .AsNoTracking()
            .Where(saved => saved.UserId == request.UserId);

        var totalCount = await savedQuery.CountAsync(ct);

        var pagePropertyIds = await savedQuery
            .OrderByDescending(saved => saved.SavedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(saved => saved.PropertyId)
            .ToListAsync(ct);

        if (pagePropertyIds.Count == 0)
        {
            return new PaginatedList<PropertyListItemDto>([], totalCount, request.PageNumber, request.PageSize);
        }

        var propertiesById = await context.Properties
            .AsNoTracking()
            .Where(property => pagePropertyIds.Contains(property.Id))
            .ToDictionaryAsync(property => property.Id, ct);

        var primaryImagesByPropertyId = await context.PropertyImages
            .AsNoTracking()
            .Where(image => image.IsPrimary && pagePropertyIds.Contains(image.PropertyId))
            .GroupBy(image => image.PropertyId)
            .Select(group => new
            {
                PropertyId = group.Key,
                Url = group
                    .OrderBy(image => image.SortOrder)
                    .Select(image => image.Url)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(image => image.PropertyId, image => image.Url, ct);

        var items = pagePropertyIds
            .Where(propertyId => propertiesById.ContainsKey(propertyId))
            .Select(propertyId =>
            {
                var property = propertiesById[propertyId];
                primaryImagesByPropertyId.TryGetValue(propertyId, out var primaryImageUrl);

                return new PropertyListItemDto(
                    property.Id,
                    property.AgentUserId,
                    property.Title,
                    property.Price,
                    property.Area,
                    property.Bedrooms,
                    property.Bathrooms,
                    property.City,
                    property.District,
                    property.PropertyType.ToString(),
                    property.ListingType.ToString(),
                    property.Status.ToString(),
                    property.IsFeatured,
                    primaryImageUrl);
            })
            .ToList();

        return new PaginatedList<PropertyListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
