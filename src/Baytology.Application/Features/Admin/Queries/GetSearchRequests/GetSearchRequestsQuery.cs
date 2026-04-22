using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetSearchRequests;

public sealed record SearchRequestAdminDto(
    Guid Id,
    string UserId,
    string InputType,
    string SearchEngine,
    string Status,
    int ResultCount,
    string? CorrelationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    int OutboxEventCount);

public sealed record GetSearchRequestsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PaginatedList<SearchRequestAdminDto>>>;
