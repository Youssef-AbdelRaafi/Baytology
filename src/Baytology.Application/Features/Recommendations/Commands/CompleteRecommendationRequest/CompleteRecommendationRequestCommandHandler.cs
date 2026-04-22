using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Recommendations;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;

public sealed class CompleteRecommendationRequestCommandHandler(IAppDbContext context)
    : IRequestHandler<CompleteRecommendationRequestCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(CompleteRecommendationRequestCommand request, CancellationToken ct)
    {
        var recommendationRequest = await context.RecommendationRequests
            .FirstOrDefaultAsync(r => r.Id == request.RecommendationRequestId, ct);

        if (recommendationRequest is null)
            return ApplicationErrors.Recommendation.CompletionRequestNotFound;

        if (recommendationRequest.Status == RequestStatus.Completed)
            return Result.Success;

        if (!request.IsSuccessful)
        {
            if (recommendationRequest.Status != RequestStatus.Failed)
            {
                recommendationRequest.Fail();
                await context.SaveChangesAsync(ct);
            }

            return Result.Success;
        }

        var incomingResults = request.Results ?? [];
        var propertyIds = incomingResults
            .Where(result => result.RecommendedPropertyId.HasValue)
            .Select(result => result.RecommendedPropertyId!.Value)
            .Distinct()
            .ToList();

        var propertySnapshots = propertyIds.Count == 0
            ? new Dictionary<Guid, PropertySnapshot>()
            : await context.Properties
                .AsNoTracking()
                .Where(property => propertyIds.Contains(property.Id))
                .Select(property => new PropertySnapshot(
                    property.Id,
                    property.Title,
                    property.Price))
                .ToDictionaryAsync(property => property.Id, ct);

        var existingResults = await context.RecommendationResults
            .Where(result => result.RequestId == request.RecommendationRequestId)
            .ToListAsync(ct);

        if (existingResults.Count > 0)
            context.RecommendationResults.RemoveRange(existingResults);

        var resolvedResults = incomingResults
            .OrderBy(result => result.Rank)
            .Select(result =>
            {
                var propertyId = result.RecommendedPropertyId;
                var snapshot = propertyId.HasValue && propertySnapshots.TryGetValue(propertyId.Value, out var existingSnapshot)
                    ? existingSnapshot
                    : null;

                return RecommendationResult.Create(
                    request.RecommendationRequestId,
                    propertyId,
                    result.ExternalReference,
                    result.SimilarityScore,
                    result.Rank,
                    result.SnapshotTitle ?? snapshot?.Title,
                    result.SnapshotPrice ?? snapshot?.Price);
            })
            .ToList();

        if (resolvedResults.Any(result => result.IsError))
            return resolvedResults.First(result => result.IsError).Errors;

        context.RecommendationResults.AddRange(resolvedResults.Select(result => result.Value));
        recommendationRequest.Complete();

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    private sealed record PropertySnapshot(
        Guid Id,
        string Title,
        decimal Price);
}
