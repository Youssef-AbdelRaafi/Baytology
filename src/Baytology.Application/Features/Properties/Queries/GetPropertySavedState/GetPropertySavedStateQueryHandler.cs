using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Queries.GetPropertySavedState;

public class GetPropertySavedStateQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPropertySavedStateQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(GetPropertySavedStateQuery request, CancellationToken ct)
    {
        var propertyExists = await context.Properties
            .AsNoTracking()
            .AnyAsync(property => property.Id == request.PropertyId, ct);

        if (!propertyExists)
            return PropertyErrors.NotFound;

        var isSaved = await context.SavedProperties
            .AsNoTracking()
            .AnyAsync(saved => saved.UserId == request.UserId && saved.PropertyId == request.PropertyId, ct);

        return isSaved;
    }
}
