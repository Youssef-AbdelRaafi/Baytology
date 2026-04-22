using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;
using Baytology.Domain.Recommendations.Events;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.Recommendations.EventHandlers;

public sealed class RecommendationRequestedEventHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<RecommendationRequestedEventHandler> logger)
    : INotificationHandler<RecommendationRequestedEvent>
{
    public async Task Handle(RecommendationRequestedEvent notification, CancellationToken ct)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dispatchPolicy = scope.ServiceProvider.GetRequiredService<IAiDispatchPolicy>();

            if (dispatchPolicy.ShouldDeferRecommendationResolution())
                return;

            var fallbackService = scope.ServiceProvider.GetRequiredService<IRecommendationFallbackService>();
            var resolution = await fallbackService.ResolveAsync(notification.RequestId, ct);

            if (resolution is null)
                return;

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(
                new CompleteRecommendationRequestCommand(
                    notification.RequestId,
                    resolution.IsSuccessful,
                    resolution.Results
                        .Select(item => new CompleteRecommendationResultInput(
                            item.RecommendedPropertyId,
                            item.ExternalReference,
                            item.SimilarityScore,
                            item.Rank,
                            item.SnapshotTitle,
                            item.SnapshotPrice))
                        .ToList()),
                ct);

            if (result.IsError)
            {
                logger.LogWarning(
                    "Internal recommendation fallback could not resolve recommendation request {RecommendationRequestId}: {ErrorCode}",
                    notification.RequestId,
                    result.TopError.Code);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Internal recommendation fallback failed after recommendation request {RecommendationRequestId} was already persisted.",
                notification.RequestId);
        }
    }
}
