namespace Baytology.Contracts.Responses.Admin;

public sealed record DomainEventLogResponse(
    Guid Id,
    string EventType,
    string AggregateId,
    string AggregateType,
    DateTimeOffset OccurredOnUtc,
    bool IsPublished,
    DateTimeOffset? PublishedOnUtc);
