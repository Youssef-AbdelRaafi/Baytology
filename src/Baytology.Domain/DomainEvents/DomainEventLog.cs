using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.DomainEvents;

public sealed class DomainEventLog : Entity
{
    public string EventType { get; private set; } = null!;
    public string AggregateId { get; private set; } = null!;
    public string AggregateType { get; private set; } = null!;
    public string? Payload { get; private set; }
    public DateTimeOffset OccurredOnUtc { get; private set; }
    public DateTimeOffset? PublishedOnUtc { get; private set; }
    public bool IsPublished { get; private set; }

    private DomainEventLog() { }

    private DomainEventLog(
        string eventType,
        string aggregateId,
        string aggregateType,
        string? payload) : base(Guid.NewGuid())
    {
        EventType = eventType;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        Payload = payload;
        OccurredOnUtc = DateTimeOffset.UtcNow;
        IsPublished = false;
    }

    public static Result<DomainEventLog> Create(
        string? eventType,
        string? aggregateId,
        string? aggregateType,
        string? payload)
    {
        var normalizedEventType = eventType?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEventType))
            return DomainEventLogErrors.EventTypeRequired;

        if (normalizedEventType.Length > 200)
            return DomainEventLogErrors.EventTypeTooLong;

        var normalizedAggregateId = aggregateId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedAggregateId))
            return DomainEventLogErrors.AggregateIdRequired;

        if (normalizedAggregateId.Length > 200)
            return DomainEventLogErrors.AggregateIdTooLong;

        var normalizedAggregateType = aggregateType?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedAggregateType))
            return DomainEventLogErrors.AggregateTypeRequired;

        if (normalizedAggregateType.Length > 100)
            return DomainEventLogErrors.AggregateTypeTooLong;

        return new DomainEventLog(normalizedEventType, normalizedAggregateId, normalizedAggregateType, payload);
    }

    public void MarkPublished()
    {
        IsPublished = true;
        PublishedOnUtc = DateTimeOffset.UtcNow;
    }
}
