using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Properties.Commands.RecordPropertyView;

public class RecordPropertyViewCommandHandler(IAppDbContext context)
    : IRequestHandler<RecordPropertyViewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordPropertyViewCommand request, CancellationToken ct)
    {
        var propertyExists = await context.Properties
            .AnyAsync(p => p.Id == request.PropertyId, ct);

        if (!propertyExists)
            return PropertyErrors.NotFound;

        var viewResult = PropertyView.Create(request.PropertyId, request.UserId, request.IpAddress);
        if (viewResult.IsError)
            return viewResult.Errors;

        var view = viewResult.Value;
        context.PropertyViews.Add(view);
        await context.SaveChangesAsync(ct);

        return view.Id;
    }
}
