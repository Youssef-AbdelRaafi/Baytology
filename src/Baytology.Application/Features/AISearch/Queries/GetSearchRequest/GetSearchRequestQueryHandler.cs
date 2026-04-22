using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.AISearch.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AISearch.Queries.GetSearchRequest;

public class GetSearchRequestQueryHandler(IAppDbContext context)
    : IRequestHandler<GetSearchRequestQuery, Result<SearchRequestDto>>
{
    public async Task<Result<SearchRequestDto>> Handle(GetSearchRequestQuery request, CancellationToken ct)
    {
        var aiRequest = await context.SearchRequests.AsNoTracking()
            .Include(sr => sr.Results)
            .FirstOrDefaultAsync(sr => sr.Id == request.Id, ct);

        if (aiRequest is null)
            return ApplicationErrors.Search.RequestNotFound;

        if (aiRequest.UserId != request.UserId)
            return ApplicationErrors.Search.AccessDenied;

        return new SearchRequestDto(
            aiRequest.Id,
            aiRequest.UserId,
            aiRequest.InputType.ToString(),
            aiRequest.SearchEngine.ToString(),
            aiRequest.Status.ToString(),
            aiRequest.ResultCount,
            aiRequest.CreatedAt,
            aiRequest.ResolvedAt,
            aiRequest.Results.OrderBy(r => r.Rank).Select(r => new SearchResultDto(
                r.PropertyId,
                r.Rank,
                r.RelevanceScore,
                r.ScoreSource,
                r.SnapshotTitle,
                r.SnapshotPrice,
                r.SnapshotCity,
                r.SnapshotStatus)).ToList());
    }
}
