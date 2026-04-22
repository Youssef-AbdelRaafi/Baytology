using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Recommendations.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Recommendations.Queries.GetRecommendationRequest;

public class GetRecommendationRequestQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRecommendationRequestQuery, Result<RecommendationRequestDto>>
{
    public async Task<Result<RecommendationRequestDto>> Handle(GetRecommendationRequestQuery request, CancellationToken ct)
    {
        var recReq = await context.RecommendationRequests.AsNoTracking()
            .Include(r => r.Results)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (recReq is null)
            return ApplicationErrors.Recommendation.RequestNotFound;

        if (recReq.RequestedByUserId != request.UserId)
            return ApplicationErrors.Recommendation.AccessDenied;

        return new RecommendationRequestDto(
            recReq.Id,
            recReq.RequestedByUserId,
            recReq.SourceEntityType,
            recReq.SourceEntityId,
            recReq.TopN,
            recReq.Status.ToString(),
            recReq.RequestedAt,
            recReq.ResolvedAt,
            recReq.Results.OrderBy(r => r.Rank).Select(r => new RecommendationResultDto(
                r.RecommendedPropertyId,
                r.ExternalReference,
                r.SimilarityScore,
                r.Rank,
                r.SnapshotTitle,
                r.SnapshotPrice)).ToList());
    }
}
