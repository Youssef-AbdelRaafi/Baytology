using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AISearch;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;

public sealed class CompleteSearchRequestCommandHandler(IAppDbContext context)
    : IRequestHandler<CompleteSearchRequestCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(CompleteSearchRequestCommand request, CancellationToken ct)
    {
        var searchRequest = await context.SearchRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.SearchRequestId, ct);

        if (searchRequest is null)
            return ApplicationErrors.Search.CompletionRequestNotFound;

        if (searchRequest.Status == RequestStatus.Completed)
            return Result.Success;

        if (!request.IsSuccessful)
        {
            if (searchRequest.Status != RequestStatus.Failed)
            {
                searchRequest.Fail();
                await context.SaveChangesAsync(ct);
            }

            return Result.Success;
        }

        var incomingResults = request.Results ?? [];
        var propertyIds = incomingResults
            .Select(result => result.PropertyId)
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
                    property.Price,
                    property.City,
                    property.Status.ToString()))
                .ToDictionaryAsync(property => property.Id, ct);

        var existingResults = await context.SearchResults
            .Where(result => result.SearchRequestId == request.SearchRequestId)
            .ToListAsync(ct);

        if (existingResults.Count > 0)
            context.SearchResults.RemoveRange(existingResults);

        var resolvedResults = incomingResults
            .OrderBy(result => result.Rank)
            .Select(result =>
            {
                propertySnapshots.TryGetValue(result.PropertyId, out var snapshot);

                return SearchResult.Create(
                    request.SearchRequestId,
                    result.PropertyId,
                    result.Rank,
                    result.RelevanceScore,
                    result.ScoreSource,
                    result.SnapshotTitle ?? snapshot?.Title,
                    result.SnapshotPrice ?? snapshot?.Price,
                    result.SnapshotCity ?? snapshot?.City,
                    result.SnapshotStatus ?? snapshot?.Status);
            })
            .ToList();

        if (resolvedResults.Any(result => result.IsError))
            return resolvedResults.First(result => result.IsError).Errors;

        context.SearchResults.AddRange(resolvedResults.Select(result => result.Value));
        searchRequest.Complete(resolvedResults.Count);

        await context.SaveChangesAsync(ct);
        return Result.Success;
    }

    private sealed record PropertySnapshot(
        Guid Id,
        string Title,
        decimal Price,
        string? City,
        string Status);
}
