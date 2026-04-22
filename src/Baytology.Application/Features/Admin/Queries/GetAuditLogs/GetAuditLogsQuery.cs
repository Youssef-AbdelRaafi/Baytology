using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetAuditLogs;

public record AuditLogDto(
    Guid Id,
    string? UserId,
    string Action,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTimeOffset OccurredOnUtc);

public record GetAuditLogsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<AuditLogDto>>>;
