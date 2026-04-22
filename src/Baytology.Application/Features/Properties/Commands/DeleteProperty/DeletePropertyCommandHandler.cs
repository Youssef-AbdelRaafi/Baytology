using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.DeleteProperty;

public class DeletePropertyCommandHandler(IAppDbContext context)
    : IRequestHandler<DeletePropertyCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeletePropertyCommand request, CancellationToken ct)
    {
        var property = await context.Properties.FirstOrDefaultAsync(p => p.Id == request.PropertyId, ct);
        if (property is null) return ApplicationErrors.Property.NotFound;
        
        if (property.AgentUserId != request.AgentUserId) 
            return ApplicationErrors.Property.AccessDenied;

        var hasRelatedActivity = await context.Bookings.AnyAsync(b => b.PropertyId == request.PropertyId, ct)
            || await context.Payments.AnyAsync(p => p.PropertyId == request.PropertyId, ct)
            || await context.Conversations.AnyAsync(c => c.PropertyId == request.PropertyId, ct)
            || await context.AgentReviews.AnyAsync(r => r.PropertyId == request.PropertyId, ct);

        if (hasRelatedActivity)
        {
            return ApplicationErrors.Property.DeleteNotAllowed;
        }

        var images = await context.PropertyImages
            .Where(i => i.PropertyId == request.PropertyId)
            .ToListAsync(ct);

        var amenity = await context.PropertyAmenities
            .FirstOrDefaultAsync(a => a.PropertyId == request.PropertyId, ct);

        var savedProperties = await context.SavedProperties
            .Where(sp => sp.PropertyId == request.PropertyId)
            .ToListAsync(ct);

        var views = await context.PropertyViews
            .Where(v => v.PropertyId == request.PropertyId)
            .ToListAsync(ct);

        if (images.Count > 0)
            context.PropertyImages.RemoveRange(images);

        if (amenity is not null)
            context.PropertyAmenities.Remove(amenity);

        if (savedProperties.Count > 0)
            context.SavedProperties.RemoveRange(savedProperties);

        if (views.Count > 0)
            context.PropertyViews.RemoveRange(views);

        context.Properties.Remove(property);
        await context.SaveChangesAsync(ct);
        return Result.Success;
    }
}
