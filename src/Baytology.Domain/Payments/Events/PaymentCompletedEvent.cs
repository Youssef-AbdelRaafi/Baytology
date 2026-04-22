using Baytology.Domain.Common;

namespace Baytology.Domain.Payments.Events;

public sealed class PaymentCompletedEvent(Guid paymentId, Guid propertyId, string payerId) : DomainEvent
{
    public Guid PaymentId { get; } = paymentId;
    public Guid PropertyId { get; } = propertyId;
    public string PayerId { get; } = payerId;
}
