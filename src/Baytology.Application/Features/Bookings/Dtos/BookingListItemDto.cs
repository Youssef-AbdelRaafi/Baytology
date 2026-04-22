namespace Baytology.Application.Features.Bookings.Dtos;

public record BookingListItemDto(
    Guid Id,
    Guid PropertyId,
    string PropertyTitle,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string Status,
    DateTimeOffset CreatedOnUtc);
