using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetRecommendationRequests;

public sealed class GetRecommendationRequestsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRecommendationRequestsQuery, Result<PaginatedList<RecommendationRequestAdminDto>>>
{
    public async Task<Result<PaginatedList<RecommendationRequestAdminDto>>> Handle(GetRecommendationRequestsQuery request, CancellationToken ct)
    {
        var query = context.RecommendationRequests
            .AsNoTracking()
            .OrderByDescending(recommendationRequest => recommendationRequest.RequestedAt)
            .Select(recommendationRequest => new RecommendationRequestAdminDto(
                recommendationRequest.Id,
                recommendationRequest.RequestedByUserId,
                recommendationRequest.SourceEntityType,
                recommendationRequest.SourceEntityId,
                recommendationRequest.TopN,
                recommendationRequest.Status.ToString(),
                recommendationRequest.CorrelationId,
                recommendationRequest.RequestedAt,
                recommendationRequest.ResolvedAt,
                context.DomainEventLogs.Count(log =>
                    log.AggregateId == recommendationRequest.Id.ToString() &&
                    log.AggregateType == nameof(Baytology.Domain.Recommendations.RecommendationRequest))));

        return await PaginatedList<RecommendationRequestAdminDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
