using System.Text.Json;

using Baytology.Domain.AISearch.Events;
using Baytology.Domain.Common;
using Baytology.Domain.DomainEvents;
using Baytology.Domain.Properties.Events;
using Baytology.Domain.Recommendations.Events;
using Baytology.Infrastructure.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Baytology.Infrastructure.Interceptors;

public class DomainEventInterceptor(
    ILogger<DomainEventInterceptor> logger,
    IPublisher publisher) : SaveChangesInterceptor
{
    private readonly ILogger<DomainEventInterceptor> _logger = logger;
    private readonly IPublisher _publisher = publisher;
    private readonly List<DomainEvent> _pendingDomainEvents = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            CaptureDomainEvents(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is AppDbContext context)
        {
            CaptureDomainEvents(context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishPendingDomainEventsAsync(cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishPendingDomainEventsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pendingDomainEvents.Clear();
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingDomainEvents.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void CaptureDomainEvents(AppDbContext context)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .Select(domainEvent => new
            {
                Event = domainEvent,
                Entity = entities.First(e => e.DomainEvents.Contains(domainEvent))
            })
            .ToList();

        if (domainEvents.Count == 0)
            return;

        _pendingDomainEvents.AddRange(domainEvents.Select(item => item.Event));
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var item in domainEvents)
        {
            if (!ShouldPersistToOutbox(item.Event))
                continue;

            var eventType = item.Event.GetType();
            var payload = JsonSerializer.Serialize(item.Event, eventType);

            _logger.LogInformation("Saving domain event to Outbox: {EventType}", eventType.Name);

            var logResult = DomainEventLog.Create(
                eventType.Name,
                item.Entity.Id.ToString(),
                item.Entity.GetType().Name,
                payload
            );

            if (logResult.IsError)
                throw new InvalidOperationException($"Failed to create domain event log for {eventType.Name}.");

            context.DomainEventLogs.Add(logResult.Value);
        }
    }

    private static bool ShouldPersistToOutbox(DomainEvent domainEvent)
    {
        return domainEvent is SearchRequestedEvent
            or RecommendationRequestedEvent
            or PropertyCreatedEvent
            or PropertyViewedEvent;
    }

    private async Task PublishPendingDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_pendingDomainEvents.Count == 0)
            return;

        var eventsToPublish = _pendingDomainEvents.ToList();
        _pendingDomainEvents.Clear();

        foreach (var domainEvent in eventsToPublish)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
