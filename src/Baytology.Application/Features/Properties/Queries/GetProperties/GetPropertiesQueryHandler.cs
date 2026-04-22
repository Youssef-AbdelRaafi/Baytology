using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetProperties;

public class GetPropertiesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPropertiesQuery, Result<PaginatedList<PropertyListItemDto>>>
{
    public async Task<Result<PaginatedList<PropertyListItemDto>>> Handle(GetPropertiesQuery request, CancellationToken ct)
    {
        var query = context.Properties
            .AsNoTracking()
            .Where(p => p.Status == Baytology.Domain.Common.Enums.PropertyStatus.Available)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(p => p.City == request.City);

        if (!string.IsNullOrWhiteSpace(request.District))
            query = query.Where(p => p.District == request.District);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.MinArea.HasValue)
            query = query.Where(p => p.Area >= request.MinArea.Value);

        if (request.MaxArea.HasValue)
            query = query.Where(p => p.Area <= request.MaxArea.Value);

        if (request.MinBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms >= request.MinBedrooms.Value);

        if (request.MaxBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms <= request.MaxBedrooms.Value);

        if (!string.IsNullOrWhiteSpace(request.PropertyType) &&
            Enum.TryParse<Baytology.Domain.Common.Enums.PropertyType>(request.PropertyType, true, out var propertyType))
        {
            query = query.Where(p => p.PropertyType == propertyType);
        }

        if (!string.IsNullOrWhiteSpace(request.ListingType) &&
            Enum.TryParse<Baytology.Domain.Common.Enums.ListingType>(request.ListingType, true, out var listingType))
        {
            query = query.Where(p => p.ListingType == listingType);
        }

        if (!string.IsNullOrWhiteSpace(request.AgentUserId))
            query = query.Where(p => p.AgentUserId == request.AgentUserId);

        var totalCount = await query.CountAsync(ct);

        if (totalCount == 0)
        {
            return new PaginatedList<PropertyListItemDto>([], 0, request.PageNumber, request.PageSize);
        }

        var pagedProperties = await query
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.AgentUserId,
                p.Title,
                p.Price,
                p.Area,
                p.Bedrooms,
                p.Bathrooms,
                p.City,
                p.District,
                PropertyType = p.PropertyType.ToString(),
                ListingType = p.ListingType.ToString(),
                Status = p.Status.ToString(),
                p.IsFeatured
            })
            .ToListAsync(ct);

        var propertyIds = pagedProperties
            .Select(property => property.Id)
            .ToArray();

        var primaryImages = await context.PropertyImages
            .AsNoTracking()
            .Where(image => propertyIds.Contains(image.PropertyId) && image.IsPrimary)
            .OrderBy(image => image.SortOrder)
            .GroupBy(image => image.PropertyId)
            .Select(group => new
            {
                PropertyId = group.Key,
                Url = group.Select(image => image.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(item => item.PropertyId, item => item.Url, ct);

        var items = pagedProperties
            .Select(property => new PropertyListItemDto(
                property.Id,
                property.AgentUserId,
                property.Title,
                property.Price,
                property.Area,
                property.Bedrooms,
                property.Bathrooms,
                property.City,
                property.District,
                property.PropertyType,
                property.ListingType,
                property.Status,
                property.IsFeatured,
                primaryImages.GetValueOrDefault(property.Id)))
            .ToList();

        return new PaginatedList<PropertyListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
