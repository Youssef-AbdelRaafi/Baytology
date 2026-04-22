using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.AddPropertyImages;

public class AddPropertyImagesCommandHandler(IAppDbContext context)
    : IRequestHandler<AddPropertyImagesCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(AddPropertyImagesCommand request, CancellationToken ct)
    {
        var property = await context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, ct);
            
        if (property is null) return ApplicationErrors.Property.NotFound;
        
        if (property.AgentUserId != request.AgentUserId) 
            return ApplicationErrors.Property.AccessDenied;

        var currentImageCount = property.Images.Count;
        
        var newImages = new List<PropertyImage>(request.ImageUrls.Count);

        for (int i = 0; i < request.ImageUrls.Count; i++)
        {
            var isPrimary = currentImageCount == 0 && i == 0;
            var imageResult = PropertyImage.Create(property.Id, request.ImageUrls[i], isPrimary, currentImageCount + i);
            if (imageResult.IsError)
                return imageResult.Errors;

            property.AddImage(imageResult.Value);
            newImages.Add(imageResult.Value);
        }

        context.PropertyImages.AddRange(newImages);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }
}
