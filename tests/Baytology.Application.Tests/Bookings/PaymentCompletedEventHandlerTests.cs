using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Bookings.EventHandlers;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Payments.Events;
using Baytology.Domain.Properties;

using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.Bookings;

public sealed class PaymentCompletedEventHandlerTests
{
    [Fact]
    public async Task Payment_completed_updates_booking_even_when_notifications_fail()
    {
        await using var context = TestDbContextFactory.Create();

        var property = Property.Create(
            "agent-1",
            "Test Property",
            "Description",
            PropertyType.Apartment,
            ListingType.Rent,
            2500m,
            120m,
            2,
            1,
            "Cairo",
            "Maadi").Value;

        var booking = Booking.Create(
            property.Id,
            "buyer-1",
            "agent-1",
            DateTimeOffset.UtcNow.AddDays(3),
            DateTimeOffset.UtcNow.AddDays(5)).Value;

        var payment = Payment.Create(
            property.Id,
            "buyer-1",
            "agent-1",
            2500m,
            0.1m,
            PaymentPurpose.Deposit,
            "EGP").Value;

        booking.AttachPayment(payment.Id);
        payment.MarkAsEscrow();

        context.Properties.Add(property);
        context.Bookings.Add(booking);
        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        var handler = new PaymentCompletedEventHandler(
            context,
            new ThrowingNotificationService(),
            NullLogger<PaymentCompletedEventHandler>.Instance);

        await handler.Handle(new PaymentCompletedEvent(payment.Id, property.Id, payment.PayerId), CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(PropertyStatus.Rented, property.Status);
    }

    private sealed class ThrowingNotificationService : INotificationService
    {
        public Task SendAsync(Notification notification, CancellationToken ct = default)
            => throw new InvalidOperationException("Notification persistence is unavailable.");
    }
}
