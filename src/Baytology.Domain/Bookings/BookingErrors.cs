using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Bookings;

public static class BookingErrors
{
    public static readonly Error PropertyRequired =
        Error.Validation("Booking.PropertyId", "Property is required.");

    public static readonly Error UserRequired =
        Error.Validation("Booking.UserId", "User is required.");

    public static readonly Error AgentRequired =
        Error.Validation("Booking.AgentUserId", "Agent is required.");

    public static readonly Error DateRangeInvalid =
        Error.Validation("Booking.DateRange", "End date must be after start date.");

    public static readonly Error StartDateInvalid =
        Error.Validation("Booking.StartDate", "Start date cannot be in the past.");
}
