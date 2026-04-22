using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
{
    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        var query = context.AuditLogs.AsNoTracking().OrderByDescending(l => l.OccurredOnUtc)
            .Select(l => new AuditLogDto(
                l.Id,
                l.UserId,
                l.Action,
                l.EntityName,
                l.EntityId,
                l.OldValues,
                l.NewValues,
                l.IpAddress,
                l.OccurredOnUtc));

        return await PaginatedList<AuditLogDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
