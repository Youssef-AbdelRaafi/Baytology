using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Bookings.Commands.CreateBooking;
using Baytology.Application.Tests.Support;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;

namespace Baytology.Application.Tests.Bookings;

public sealed class CreateBookingCommandHandlerTests
{
    [Fact]
    public async Task Handle_returns_conflict_when_booking_dates_overlap_existing_booking()
    {
        await using var context = TestDbContextFactory.Create();
        var paymentGateway = new TestPaymentGateway(new PaymentIntentionResponse("int-1", "secret", "https://pay.test/redirect"));
        var notifications = new TestNotificationService();
        var handler = new CreateBookingCommandHandler(context, paymentGateway, notifications);

        var property = CreateProperty();
        var existingBooking = Booking.Create(
            property.Id,
            "buyer-existing",
            property.AgentUserId,
            DateTimeOffset.UtcNow.AddDays(10),
            DateTimeOffset.UtcNow.AddDays(15)).Value;

        context.Properties.Add(property);
        context.AgentDetails.Add(AgentDetail.Create(property.AgentUserId, commissionRate: 0.03m).Value);
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync();

        var command = new CreateBookingCommand(
            property.Id,
            "buyer-new",
            DateTimeOffset.UtcNow.AddDays(12),
            DateTimeOffset.UtcNow.AddDays(18),
            4200m,
            0.03m,
            "EGP",
            "buyer@test.local",
            "Buyer New",
            "01000000001");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Booking.Overlapping", result.TopError.Code);
        Assert.Equal(0, paymentGateway.CallCount);
        Assert.Empty(notifications.SentNotifications);
    }

    [Fact]
    public async Task Handle_marks_payment_failed_and_booking_cancelled_when_gateway_creation_fails()
    {
        await using var context = TestDbContextFactory.Create();
        var paymentGateway = new TestPaymentGateway(Error.Failure("PaymentGateway.Unavailable", "Gateway is unavailable."));
        var notifications = new TestNotificationService();
        var handler = new CreateBookingCommandHandler(context, paymentGateway, notifications);

        var property = CreateProperty();
        var agentDetail = AgentDetail.Create(property.AgentUserId, commissionRate: 0.04m).Value;

        context.Properties.Add(property);
        context.AgentDetails.Add(agentDetail);
        await context.SaveChangesAsync();

        var command = new CreateBookingCommand(
            property.Id,
            "buyer-new",
            DateTimeOffset.UtcNow.AddDays(12),
            DateTimeOffset.UtcNow.AddDays(18),
            4200m,
            0m,
            "EGP",
            "buyer@test.local",
            "Buyer New",
            "01000000001");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("PaymentGateway.Unavailable", result.TopError.Code);
        Assert.Equal(1, paymentGateway.CallCount);
        Assert.Empty(notifications.SentNotifications);

        var booking = Assert.Single(context.Bookings);
        var payment = Assert.Single(context.Payments);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(payment.Id, booking.PaymentId);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Empty(context.PaymentTransactions);
    }

    private static Property CreateProperty()
    {
        return Property.Create(
            "agent-1",
            "Available Listing",
            "Property for testing",
            PropertyType.Apartment,
            ListingType.Rent,
            15000m,
            160m,
            3,
            2,
            "Cairo",
            "New Cairo").Value;
    }
}
