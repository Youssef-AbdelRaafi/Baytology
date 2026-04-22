using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Recommendations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;

public sealed record CompleteRecommendationResultInput(
    Guid? RecommendedPropertyId,
    string? ExternalReference,
    float SimilarityScore,
    int Rank,
    string? SnapshotTitle,
    decimal? SnapshotPrice);

public sealed record CompleteRecommendationRequestCommand(
    Guid RecommendationRequestId,
    bool IsSuccessful,
    List<CompleteRecommendationResultInput>? Results) : IRequest<Result<Success>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.RecommendationRequests,
        ApplicationCacheTags.RecommendationRequest(RecommendationRequestId)
    ];
}
