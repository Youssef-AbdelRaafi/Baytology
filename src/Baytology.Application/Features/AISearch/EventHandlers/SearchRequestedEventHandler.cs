using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;
using Baytology.Domain.AISearch.Events;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.AISearch.EventHandlers;

public sealed class SearchRequestedEventHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<SearchRequestedEventHandler> logger)
    : INotificationHandler<SearchRequestedEvent>
{
    public async Task Handle(SearchRequestedEvent notification, CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dispatchPolicy = scope.ServiceProvider.GetRequiredService<IAiDispatchPolicy>();

            if (dispatchPolicy.ShouldDeferSearchResolution(notification.InputType))
                return;

            var fallbackService = scope.ServiceProvider.GetRequiredService<IAiSearchFallbackService>();
            var resolution = await fallbackService.ResolveAsync(notification.SearchRequestId, ct);

            if (resolution is null)
                return;

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(
                new CompleteSearchRequestCommand(
                    notification.SearchRequestId,
                    resolution.IsSuccessful,
                    resolution.Results
                        .Select(item => new CompleteSearchResultInput(
                            item.PropertyId,
                            item.Rank,
                            item.RelevanceScore,
                            item.ScoreSource,
                            item.SnapshotTitle,
                            item.SnapshotPrice,
                            item.SnapshotCity,
                            item.SnapshotStatus))
                        .ToList()),
                ct);

            if (result.IsError)
            {
                logger.LogWarning(
                    "Internal AI search fallback could not resolve search request {SearchRequestId}: {ErrorCode}",
                    notification.SearchRequestId,
                    result.TopError.Code);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Internal AI search fallback failed after search request {SearchRequestId} was already persisted.",
                notification.SearchRequestId);
        }
    }
}
