using Baytology.Domain.Common.Results;

namespace Baytology.Domain.DomainEvents;

public static class DomainEventLogErrors
{
    public static readonly Error EventTypeRequired =
        Error.Validation("DomainEventLog_EventType_Required", "Event type is required.");

    public static readonly Error EventTypeTooLong =
        Error.Validation("DomainEventLog_EventType_TooLong", "Event type cannot exceed 200 characters.");

    public static readonly Error AggregateIdRequired =
        Error.Validation("DomainEventLog_AggregateId_Required", "Aggregate id is required.");

    public static readonly Error AggregateIdTooLong =
        Error.Validation("DomainEventLog_AggregateId_TooLong", "Aggregate id cannot exceed 200 characters.");

    public static readonly Error AggregateTypeRequired =
        Error.Validation("DomainEventLog_AggregateType_Required", "Aggregate type is required.");

    public static readonly Error AggregateTypeTooLong =
        Error.Validation("DomainEventLog_AggregateType_TooLong", "Aggregate type cannot exceed 100 characters.");
}
