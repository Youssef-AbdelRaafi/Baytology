namespace Baytology.Contracts.Requests.Bookings;

public sealed record CreateBookingRequest(
    Guid PropertyId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Amount,
    decimal CommissionRate,
    string Currency,
    string PayerEmail,
    string PayerName,
    string PayerPhone);
