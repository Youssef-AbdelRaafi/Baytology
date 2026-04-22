using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AISearch;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;

public sealed record CompleteSearchResultInput(
    Guid PropertyId,
    int Rank,
    float RelevanceScore,
    string? ScoreSource,
    string? SnapshotTitle,
    decimal? SnapshotPrice,
    string? SnapshotCity,
    string? SnapshotStatus);

public sealed record CompleteSearchRequestCommand(
    Guid SearchRequestId,
    bool IsSuccessful,
    List<CompleteSearchResultInput>? Results) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.SearchRequests,
        ApplicationCacheTags.SearchRequest(SearchRequestId)
    ];
}
