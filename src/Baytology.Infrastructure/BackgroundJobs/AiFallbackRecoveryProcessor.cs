using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;
using Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.Settings;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.BackgroundJobs;

public sealed class AiFallbackRecoveryProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AiProcessingSettings> aiProcessingOptions,
    IOptions<RabbitMqSettings> rabbitOptions,
    ILogger<AiFallbackRecoveryProcessor> logger) : BackgroundService
{
    private readonly AiProcessingSettings _aiProcessingSettings = aiProcessingOptions.Value;
    private readonly RabbitMqSettings _rabbitSettings = rabbitOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_aiProcessingSettings.EnableInProcessFallback || !_aiProcessingSettings.EnableDelayedFallbackRecovery)
            return;

        logger.LogInformation("AI fallback recovery processor starting...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecoverPendingRequestsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during delayed AI fallback recovery.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("AI fallback recovery processor stopped.");
    }

    private async Task RecoverPendingRequestsAsync(CancellationToken ct)
    {
        if (!_rabbitSettings.Enabled)
            return;

        var gracePeriod = TimeSpan.FromSeconds(Math.Max(1, _aiProcessingSettings.ExternalWorkerGracePeriodSeconds));
        var cutoff = DateTimeOffset.UtcNow.Subtract(gracePeriod);

        await RecoverSearchRequestsAsync(cutoff, ct);
        await RecoverRecommendationRequestsAsync(cutoff, ct);
    }

    private async Task RecoverSearchRequestsAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var fallbackService = scope.ServiceProvider.GetRequiredService<IAiSearchFallbackService>();

        var pendingIds = await context.SearchRequests
            .AsNoTracking()
            .Where(request => request.Status == RequestStatus.Pending && request.CreatedAt <= cutoff)
            .OrderBy(request => request.CreatedAt)
            .Select(request => request.Id)
            .Take(10)
            .ToListAsync(ct);

        foreach (var searchRequestId in pendingIds)
        {
            var resolution = await fallbackService.ResolveAsync(searchRequestId, ct);

            if (resolution is null)
                continue;

            var result = await sender.Send(
                new CompleteSearchRequestCommand(
                    searchRequestId,
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

            if (result.IsSuccess)
            {
                logger.LogInformation("Recovered pending AI search request {SearchRequestId} via delayed fallback.", searchRequestId);
            }
        }
    }

    private async Task RecoverRecommendationRequestsAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var fallbackService = scope.ServiceProvider.GetRequiredService<IRecommendationFallbackService>();

        var pendingIds = await context.RecommendationRequests
            .AsNoTracking()
            .Where(request => request.Status == RequestStatus.Pending && request.RequestedAt <= cutoff)
            .OrderBy(request => request.RequestedAt)
            .Select(request => request.Id)
            .Take(10)
            .ToListAsync(ct);

        foreach (var requestId in pendingIds)
        {
            var resolution = await fallbackService.ResolveAsync(requestId, ct);

            if (resolution is null)
                continue;

            var result = await sender.Send(
                new CompleteRecommendationRequestCommand(
                    requestId,
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

            if (result.IsSuccess)
            {
                logger.LogInformation("Recovered pending recommendation request {RecommendationRequestId} via delayed fallback.", requestId);
            }
        }
    }
}
