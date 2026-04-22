using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetDomainEventLogs;

public class GetDomainEventLogsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetDomainEventLogsQuery, Result<PaginatedList<DomainEventLogDto>>>
{
    public async Task<Result<PaginatedList<DomainEventLogDto>>> Handle(GetDomainEventLogsQuery request, CancellationToken ct)
    {
        var query = context.DomainEventLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredOnUtc)
            .Select(log => new DomainEventLogDto(
                log.Id,
                log.EventType,
                log.AggregateId,
                log.AggregateType,
                log.OccurredOnUtc,
                log.IsPublished,
                log.PublishedOnUtc));

        return await PaginatedList<DomainEventLogDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
