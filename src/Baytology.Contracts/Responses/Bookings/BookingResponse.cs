namespace Baytology.Contracts.Responses.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid PropertyId,
    string PropertyTitle,
    string UserId,
    string AgentUserId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Status,
    Guid? PaymentId,
    DateTimeOffset CreatedOnUtc);
