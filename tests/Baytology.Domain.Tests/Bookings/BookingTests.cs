using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;

namespace Baytology.Domain.Tests.Bookings;

public sealed class BookingTests
{
    [Fact]
    public void Create_returns_validation_error_when_start_date_is_in_the_past()
    {
        var result = Booking.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));

        Assert.True(result.IsError);
        Assert.Equal(BookingErrors.StartDateInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_returns_validation_error_when_start_time_is_earlier_today()
    {
        var result = Booking.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddDays(1));

        Assert.True(result.IsError);
        Assert.Equal(BookingErrors.StartDateInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_sets_pending_status_for_valid_future_booking()
    {
        var result = Booking.Create(
            Guid.NewGuid(),
            "buyer-1",
            "agent-1",
            DateTimeOffset.UtcNow.AddDays(5),
            DateTimeOffset.UtcNow.AddDays(7));

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Pending, result.Value.Status);
    }
}
