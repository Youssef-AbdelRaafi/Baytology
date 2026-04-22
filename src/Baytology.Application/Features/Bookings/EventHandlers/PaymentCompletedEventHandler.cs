using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.Bookings.EventHandlers;

public sealed class PaymentCompletedEventHandler(
    IAppDbContext context,
    INotificationService notificationService,
    ILogger<PaymentCompletedEventHandler> logger)
    : INotificationHandler<PaymentCompletedEvent>
{
    public async Task Handle(PaymentCompletedEvent notification, CancellationToken ct)
    {
        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.PaymentId == notification.PaymentId, ct);

        if (booking is null)
            return;

        if (booking.Status != BookingStatus.Pending)
            return;

        booking.Confirm();

        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == booking.PropertyId, ct);

        property?.MarkUnavailableForConfirmedBooking();

        await context.SaveChangesAsync(ct);

        var buyerNotificationResult = Notification.Create(
            booking.UserId,
            NotificationType.PaymentUpdate,
            "Booking confirmed",
            "Your payment was completed and the booking is now confirmed.",
            booking.Id.ToString(),
            ReferenceType.Booking);

        if (buyerNotificationResult.IsError)
            return;

        await TrySendNotificationAsync(buyerNotificationResult.Value, booking.Id, ct);

        var agentNotificationResult = Notification.Create(
            booking.AgentUserId,
            NotificationType.PaymentUpdate,
            "Payment completed",
            "The booking payment was completed and the property status was updated.",
            booking.Id.ToString(),
            ReferenceType.Booking);

        if (agentNotificationResult.IsError)
            return;

        await TrySendNotificationAsync(agentNotificationResult.Value, booking.Id, ct);
    }

    private async Task TrySendNotificationAsync(Notification notification, Guid bookingId, CancellationToken ct)
    {
        try
        {
            await notificationService.SendAsync(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to persist or deliver payment completion notification for booking {BookingId}.",
                bookingId);
        }
    }
}
