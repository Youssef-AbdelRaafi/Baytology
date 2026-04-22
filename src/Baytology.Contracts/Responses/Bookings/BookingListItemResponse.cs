namespace Baytology.Contracts.Responses.Bookings;

public sealed record BookingListItemResponse(
    Guid Id,
    Guid PropertyId,
    string PropertyTitle,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Status,
    DateTimeOffset CreatedOnUtc);
