using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

namespace Baytology.Application.Features.Properties.Commands.CreateProperty;

public class CreatePropertyCommandHandler(IAppDbContext context)
    : IRequestHandler<CreatePropertyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken ct)
    {
        var propertyResult = Property.Create(
            request.AgentUserId,
            request.Title,
            request.Description,
            request.PropertyType,
            request.ListingType,
            request.Price,
            request.Area,
            request.Bedrooms,
            request.Bathrooms,
            request.City,
            request.District,
            floor: request.Floor,
            totalFloors: request.TotalFloors);

        if (propertyResult.IsError)
            return propertyResult.Errors;

        var property = propertyResult.Value;

        property.SetLocation(
            request.AddressLine, request.City, request.District,
            request.ZipCode, request.Latitude, request.Longitude);

        var amenityResult = PropertyAmenity.Create(property.Id);
        if (amenityResult.IsError)
            return amenityResult.Errors;

        var amenity = amenityResult.Value;
        amenity.Update(
            request.HasParking,
            request.HasPool,
            request.HasGym,
            request.HasElevator,
            request.HasSecurity,
            request.HasBalcony,
            request.HasGarden,
            request.HasCentralAC,
            request.FurnishingStatus,
            request.ViewType);

        context.PropertyAmenities.Add(amenity);

        if (request.ImageUrls is not null)
        {
            for (var i = 0; i < request.ImageUrls.Count; i++)
            {
                var imageResult = PropertyImage.Create(property.Id, request.ImageUrls[i], i == 0, i);
                if (imageResult.IsError)
                    return imageResult.Errors;

                property.AddImage(imageResult.Value);
            }
        }

        context.Properties.Add(property);
        await context.SaveChangesAsync(ct);

        return property.Id;
    }
}
