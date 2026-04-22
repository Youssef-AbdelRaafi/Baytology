using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Bookings;

public sealed class Booking : AuditableEntity
{
    public Guid PropertyId { get; private set; }
    public string UserId { get; private set; } = null!;
    public string AgentUserId { get; private set; } = null!;
    public Guid? PaymentId { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public BookingStatus Status { get; private set; }

    private Booking() { }

    private Booking(
        Guid id,
        Guid propertyId,
        string userId,
        string agentUserId,
        DateTimeOffset startDate,
        DateTimeOffset endDate) : base(id)
    {
        PropertyId = propertyId;
        UserId = userId;
        AgentUserId = agentUserId;
        StartDate = startDate;
        EndDate = endDate;
        Status = BookingStatus.Pending;
    }

    public static Result<Booking> Create(
        Guid propertyId,
        string userId,
        string agentUserId,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        if (propertyId == Guid.Empty)
            return BookingErrors.PropertyRequired;

        if (string.IsNullOrWhiteSpace(userId))
            return BookingErrors.UserRequired;

        if (string.IsNullOrWhiteSpace(agentUserId))
            return BookingErrors.AgentRequired;

        if (endDate <= startDate)
            return BookingErrors.DateRangeInvalid;

        if (startDate < DateTimeOffset.UtcNow)
            return BookingErrors.StartDateInvalid;

        return new Booking(Guid.NewGuid(), propertyId, userId, agentUserId, startDate, endDate);
    }

    public void AttachPayment(Guid paymentId) => PaymentId = paymentId;

    public void Confirm() => Status = BookingStatus.Confirmed;

    public void Cancel() => Status = BookingStatus.Cancelled;
}
