using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetRecommendationRequests;

public sealed record RecommendationRequestAdminDto(
    Guid Id,
    string RequestedByUserId,
    string SourceEntityType,
    string? SourceEntityId,
    int TopN,
    string Status,
    string? CorrelationId,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ResolvedAt,
    int OutboxEventCount);

public sealed record GetRecommendationRequestsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PaginatedList<RecommendationRequestAdminDto>>>;
