using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Queries.GetSearchRequests;

public sealed class GetSearchRequestsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetSearchRequestsQuery, Result<PaginatedList<SearchRequestAdminDto>>>
{
    public async Task<Result<PaginatedList<SearchRequestAdminDto>>> Handle(GetSearchRequestsQuery request, CancellationToken ct)
    {
        var query = context.SearchRequests
            .AsNoTracking()
            .OrderByDescending(searchRequest => searchRequest.CreatedAt)
            .Select(searchRequest => new SearchRequestAdminDto(
                searchRequest.Id,
                searchRequest.UserId,
                searchRequest.InputType.ToString(),
                searchRequest.SearchEngine.ToString(),
                searchRequest.Status.ToString(),
                searchRequest.ResultCount,
                searchRequest.CorrelationId,
                searchRequest.CreatedAt,
                searchRequest.ResolvedAt,
                context.DomainEventLogs.Count(log =>
                    log.AggregateId == searchRequest.Id.ToString() &&
                    log.AggregateType == nameof(Baytology.Domain.AISearch.SearchRequest))));

        return await PaginatedList<SearchRequestAdminDto>.CreateAsync(query, request.PageNumber, request.PageSize, ct);
    }
}
