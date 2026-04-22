using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetPropertyById;

public class GetPropertyByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDto>>
{
    public async Task<Result<PropertyDto>> Handle(GetPropertyByIdQuery request, CancellationToken ct)
    {
        var property = await context.Properties
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Amenity)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (property is null)
            return PropertyErrors.NotFound;

        var agentProfile = await context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == property.AgentUserId, ct);

        var agentDetail = await context.AgentDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == property.AgentUserId, ct);

        return new PropertyDto(
            property.Id,
            property.AgentUserId,
            property.Title,
            property.Description,
            property.PropertyType.ToString(),
            property.ListingType.ToString(),
            property.Price,
            property.Area,
            property.Bedrooms,
            property.Bathrooms,
            property.Floor,
            property.TotalFloors,
            property.AddressLine,
            property.City,
            property.District,
            property.ZipCode,
            property.Latitude,
            property.Longitude,
            property.Status.ToString(),
            property.IsFeatured,
            property.CreatedOnUtc,
            property.Images.Select(i => new PropertyImageDto(i.Id, i.Url, i.IsPrimary, i.SortOrder)).ToList(),
            property.Amenity is null ? null : new PropertyAmenityDto(
                property.Amenity.HasParking, property.Amenity.HasPool,
                property.Amenity.HasGym, property.Amenity.HasElevator,
                property.Amenity.HasSecurity, property.Amenity.HasBalcony,
                property.Amenity.HasGarden, property.Amenity.HasCentralAC,
                property.Amenity.FurnishingStatus.ToString(),
                property.Amenity.ViewType?.ToString()),
            agentDetail is null
                ? null
                : new AgentSummaryDto(
                    property.AgentUserId,
                    agentProfile?.DisplayName,
                    agentProfile?.AvatarUrl,
                    agentDetail.AgencyName,
                    agentDetail.Rating,
                    agentDetail.ReviewCount,
                    agentDetail.IsVerified));
    }
}
