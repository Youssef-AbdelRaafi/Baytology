using System.Text.Json;

using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AISearch.Events;
using Baytology.Domain.AISearch;
using Baytology.Domain.DomainEvents;
using Baytology.Domain.Properties.Events;
using Baytology.Domain.Properties;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.Settings;
using Baytology.Domain.Recommendations.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.BackgroundJobs;

public class OutboxProcessor(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitOptions,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly RabbitMqSettings _rabbitSettings = rabbitOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor starting...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
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

        logger.LogInformation("Outbox processor stopped.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var messages = await context.DomainEventLogs
            .Where(m => !m.IsPublished)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                if (!await TryPublishIntegrationEventAsync(context, message, messagePublisher, _rabbitSettings, ct))
                {
                    logger.LogWarning("Unsupported outbox message type: {EventType}", message.EventType);
                    continue;
                }

                message.MarkPublished();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish outbox message {MessageId} of type {EventType}", message.Id, message.EventType);
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task<bool> TryPublishIntegrationEventAsync(
        AppDbContext context,
        DomainEventLog message,
        IMessagePublisher messagePublisher,
        RabbitMqSettings rabbitSettings,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message.Payload))
            return false;

        if (message.EventType == nameof(SearchRequestedEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<SearchRequestedEvent>(message.Payload);
            if (domainEvent is null)
                return false;

            var workerMessage = await BuildSearchRequestedMessageAsync(context, domainEvent, ct);
            if (workerMessage is null)
                return false;

            await messagePublisher.PublishAsync(rabbitSettings.SearchQueueName, workerMessage, ct);

            return true;
        }

        if (message.EventType == nameof(RecommendationRequestedEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<RecommendationRequestedEvent>(message.Payload);
            if (domainEvent is null)
                return false;

            var workerMessage = await BuildRecommendationRequestedMessageAsync(context, domainEvent, ct);
            if (workerMessage is null)
                return false;

            await messagePublisher.PublishAsync(rabbitSettings.RecommendationQueueName, workerMessage, ct);

            return true;
        }

        if (message.EventType == nameof(PropertyCreatedEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<PropertyCreatedEvent>(message.Payload);
            if (domainEvent is null)
                return false;

            await messagePublisher.PublishAsync(rabbitSettings.PropertyIndexQueueName, new PropertyIndexedMessage(
                domainEvent.PropertyId,
                DateTimeOffset.UtcNow), ct);

            return true;
        }

        if (message.EventType == nameof(PropertyViewedEvent))
        {
            var domainEvent = JsonSerializer.Deserialize<PropertyViewedEvent>(message.Payload);
            if (domainEvent is null)
                return false;

            await messagePublisher.PublishAsync(rabbitSettings.UserHistoryQueueName, new PropertyViewedMessage(
                domainEvent.PropertyId,
                domainEvent.UserId,
                DateTimeOffset.UtcNow), ct);

            return true;
        }

        return false;
    }

    private static async Task<SearchRequestedMessage?> BuildSearchRequestedMessageAsync(
        AppDbContext context,
        SearchRequestedEvent domainEvent,
        CancellationToken ct)
    {
        var searchRequest = await context.SearchRequests
            .AsNoTracking()
            .Include(request => request.TextSearch)
            .Include(request => request.VoiceSearch)
            .Include(request => request.ImageSearch)
            .Include(request => request.Filter)
            .FirstOrDefaultAsync(request => request.Id == domainEvent.SearchRequestId, ct);

        if (searchRequest is null)
            return null;

        return new SearchRequestedMessage(
            searchRequest.Id,
            searchRequest.UserId,
            searchRequest.InputType.ToString(),
            searchRequest.SearchEngine.ToString(),
            searchRequest.CorrelationId,
            searchRequest.TextSearch?.RawQuery,
            searchRequest.VoiceSearch?.AudioFileUrl,
            searchRequest.ImageSearch?.ImageFileUrl,
            new SearchFilterMessage(
                searchRequest.Filter?.City,
                searchRequest.Filter?.District,
                searchRequest.Filter?.PropertyType,
                searchRequest.Filter?.ListingType,
                searchRequest.Filter?.MinPrice,
                searchRequest.Filter?.MaxPrice,
                searchRequest.Filter?.MinArea,
                searchRequest.Filter?.MaxArea,
                searchRequest.Filter?.MinBedrooms,
                searchRequest.Filter?.MaxBedrooms),
            $"/api/internal/ai/search/{searchRequest.Id:D}/resolve");
    }

    private static async Task<RecommendationRequestedMessage?> BuildRecommendationRequestedMessageAsync(
        AppDbContext context,
        RecommendationRequestedEvent domainEvent,
        CancellationToken ct)
    {
        var sourceProperty = await ResolveRecommendationSourcePropertyAsync(context, domainEvent, ct);

        return new RecommendationRequestedMessage(
            domainEvent.RequestId,
            domainEvent.UserId,
            domainEvent.SourceEntityType,
            domainEvent.SourceEntityId,
            domainEvent.TopN,
            domainEvent.CorrelationId,
            sourceProperty,
            $"/api/internal/ai/recommendations/{domainEvent.RequestId:D}/resolve");
    }

    private static async Task<ExternalPropertyReferenceMessage?> ResolveRecommendationSourcePropertyAsync(
        AppDbContext context,
        RecommendationRequestedEvent domainEvent,
        CancellationToken ct)
    {
        if (domainEvent.SourceEntityType.Contains("property", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(domainEvent.SourceEntityId, out var propertyId))
        {
            return await LoadPropertyReferenceAsync(context, propertyId, ct);
        }

        if (domainEvent.SourceEntityType.Contains("search", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(domainEvent.SourceEntityId, out var searchRequestId))
        {
            var topSearchPropertyId = await context.SearchResults
                .AsNoTracking()
                .Where(result => result.SearchRequestId == searchRequestId)
                .OrderBy(result => result.Rank)
                .Select(result => (Guid?)result.PropertyId)
                .FirstOrDefaultAsync(ct);

            if (topSearchPropertyId.HasValue)
                return await LoadPropertyReferenceAsync(context, topSearchPropertyId.Value, ct);
        }

        if (domainEvent.SourceEntityType.Contains("history", StringComparison.OrdinalIgnoreCase) ||
            domainEvent.SourceEntityType.Contains("user", StringComparison.OrdinalIgnoreCase))
        {
            var latestViewedPropertyId = await context.PropertyViews
                .AsNoTracking()
                .Where(view => view.UserId == domainEvent.UserId)
                .OrderByDescending(view => view.ViewedAt)
                .Select(view => (Guid?)view.PropertyId)
                .FirstOrDefaultAsync(ct)
                ?? await context.SavedProperties
                    .AsNoTracking()
                    .Where(saved => saved.UserId == domainEvent.UserId)
                    .OrderByDescending(saved => saved.SavedAt)
                    .Select(saved => (Guid?)saved.PropertyId)
                    .FirstOrDefaultAsync(ct)
                ?? await context.Bookings
                    .AsNoTracking()
                    .Where(booking => booking.UserId == domainEvent.UserId)
                    .OrderByDescending(booking => booking.CreatedOnUtc)
                    .Select(booking => (Guid?)booking.PropertyId)
                    .FirstOrDefaultAsync(ct);

            if (latestViewedPropertyId.HasValue)
                return await LoadPropertyReferenceAsync(context, latestViewedPropertyId.Value, ct);
        }

        return null;
    }

    private static async Task<ExternalPropertyReferenceMessage?> LoadPropertyReferenceAsync(
        AppDbContext context,
        Guid propertyId,
        CancellationToken ct)
    {
        return await context.Properties
            .AsNoTracking()
            .Where(property => property.Id == propertyId)
            .Select(property => new ExternalPropertyReferenceMessage(
                property.Id,
                property.SourceListingUrl,
                property.Title,
                property.Price,
                property.City,
                property.District,
                property.PropertyType.ToString(),
                property.Area,
                property.Bedrooms))
            .FirstOrDefaultAsync(ct);
    }

    private sealed record SearchRequestedMessage(
        Guid SearchRequestId,
        string UserId,
        string InputType,
        string SearchEngine,
        string? CorrelationId,
        string? RawQuery,
        string? AudioFileUrl,
        string? ImageFileUrl,
        SearchFilterMessage Filter,
        string ResolveCallbackPath);

    private sealed record RecommendationRequestedMessage(
        Guid RequestId,
        string UserId,
        string SourceEntityType,
        string? SourceEntityId,
        int TopN,
        string? CorrelationId,
        ExternalPropertyReferenceMessage? SourceProperty,
        string ResolveCallbackPath);

    private sealed record SearchFilterMessage(
        string? City,
        string? District,
        string? PropertyType,
        string? ListingType,
        decimal? MinPrice,
        decimal? MaxPrice,
        decimal? MinArea,
        decimal? MaxArea,
        int? MinBedrooms,
        int? MaxBedrooms);

    private sealed record ExternalPropertyReferenceMessage(
        Guid InternalPropertyId,
        string? SourceListingUrl,
        string? Title,
        decimal? Price,
        string? City,
        string? District,
        string? PropertyType,
        decimal? Area,
        int? Bedrooms);

    private sealed record PropertyIndexedMessage(
        Guid PropertyId,
        DateTimeOffset OccurredAtUtc);

    private sealed record PropertyViewedMessage(
        Guid PropertyId,
        string? UserId,
        DateTimeOffset OccurredAtUtc);
}
