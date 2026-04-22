using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetDomainEventLogs;

public record DomainEventLogDto(
    Guid Id,
    string EventType,
    string AggregateId,
    string AggregateType,
    DateTimeOffset OccurredOnUtc,
    bool IsPublished,
    DateTimeOffset? PublishedOnUtc);

public record GetDomainEventLogsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<DomainEventLogDto>>>;
