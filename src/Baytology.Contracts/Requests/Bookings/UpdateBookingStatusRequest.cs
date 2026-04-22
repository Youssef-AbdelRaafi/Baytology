using Baytology.Contracts.Common;

namespace Baytology.Contracts.Requests.Bookings;

public sealed record UpdateBookingStatusRequest(BookingStatus Status);
