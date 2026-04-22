namespace Baytology.Contracts.Responses.Bookings;

public sealed record CreateBookingResponse(
    Guid BookingId,
    Guid PaymentId,
    string? RedirectUrl);
