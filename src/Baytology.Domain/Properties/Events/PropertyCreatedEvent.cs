using Baytology.Domain.Common;

namespace Baytology.Domain.Properties.Events;

public sealed class PropertyCreatedEvent(Guid propertyId) : DomainEvent
{
    public Guid PropertyId { get; } = propertyId;
}
