using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.UnsaveProperty;

public class UnsavePropertyCommandHandler(IAppDbContext context)
    : IRequestHandler<UnsavePropertyCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UnsavePropertyCommand request, CancellationToken ct)
    {
        var savedProperty = await context.SavedProperties
            .FirstOrDefaultAsync(saved => saved.UserId == request.UserId && saved.PropertyId == request.PropertyId, ct);

        if (savedProperty is null)
            return PropertyErrors.NotSaved;

        context.SavedProperties.Remove(savedProperty);
        await context.SaveChangesAsync(ct);

        return Result.Success;
    }
}
