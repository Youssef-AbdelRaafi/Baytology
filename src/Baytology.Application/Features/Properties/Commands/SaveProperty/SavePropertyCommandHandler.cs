using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.SaveProperty;

public class SavePropertyCommandHandler(IAppDbContext context)
    : IRequestHandler<SavePropertyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SavePropertyCommand request, CancellationToken ct)
    {
        var propertyExists = await context.Properties
            .AnyAsync(p => p.Id == request.PropertyId, ct);

        if (!propertyExists)
            return PropertyErrors.NotFound;

        var exists = await context.SavedProperties
            .AnyAsync(s => s.UserId == request.UserId && s.PropertyId == request.PropertyId, ct);

        if (exists)
            return PropertyErrors.AlreadySaved;

        var savedResult = SavedProperty.Create(request.UserId, request.PropertyId);
        if (savedResult.IsError)
            return savedResult.Errors;

        var saved = savedResult.Value;
        context.SavedProperties.Add(saved);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var duplicateExists = await context.SavedProperties
                .AnyAsync(s => s.UserId == request.UserId && s.PropertyId == request.PropertyId, ct);

            if (duplicateExists)
                return PropertyErrors.AlreadySaved;

            throw;
        }

        return saved.Id;
    }
}
