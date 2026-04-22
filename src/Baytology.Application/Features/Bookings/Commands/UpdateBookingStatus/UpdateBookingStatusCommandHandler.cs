using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Commands.UpdateBookingStatus;

public sealed class UpdateBookingStatusCommandHandler(
    IAppDbContext context,
    INotificationService notificationService)
    : IRequestHandler<UpdateBookingStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateBookingStatusCommand request, CancellationToken ct)
    {
        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, ct);

        if (booking is null)
            return ApplicationErrors.Booking.StatusUpdateNotFound;

        var isAgent = booking.AgentUserId == request.ActorUserId;
        var isBookingOwner = booking.UserId == request.ActorUserId;

        if (!isAgent && !isBookingOwner)
            return ApplicationErrors.Booking.AccessDenied;

        Notification? notification = null;

        if (request.NewStatus == BookingStatus.Confirmed)
        {
            if (!isAgent)
                return ApplicationErrors.Booking.NotAgent;

            if (booking.Status == BookingStatus.Confirmed)
                return true;

            if (!booking.PaymentId.HasValue)
                return ApplicationErrors.Booking.NoPayment;

            var payment = await context.Payments
                .FirstOrDefaultAsync(p => p.Id == booking.PaymentId.Value, ct);

            if (payment is null || payment.Status != PaymentStatus.Completed)
                return ApplicationErrors.Booking.PaymentNotCompleted;

            booking.Confirm();

            var property = await context.Properties
                .FirstOrDefaultAsync(p => p.Id == booking.PropertyId, ct);

            property?.MarkUnavailableForConfirmedBooking();

            var notificationResult = Notification.Create(
                booking.UserId,
                NotificationType.PaymentUpdate,
                "Booking confirmed",
                "Your booking has been confirmed.",
                booking.Id.ToString(),
                ReferenceType.Booking);

            if (notificationResult.IsError)
                return notificationResult.Errors;

            notification = notificationResult.Value;
        }
        else if (request.NewStatus == BookingStatus.Cancelled)
        {
            if (booking.Status == BookingStatus.Cancelled)
                return true;

            if (booking.Status == BookingStatus.Confirmed)
                return ApplicationErrors.Booking.AlreadyConfirmed;

            booking.Cancel();

            var notificationRecipientId = isAgent ? booking.UserId : booking.AgentUserId;
            var notificationBody = isAgent
                ? "Your booking request has been cancelled by the agent."
                : "The buyer cancelled the booking request.";

            var notificationResult = Notification.Create(
                notificationRecipientId,
                NotificationType.PaymentUpdate,
                "Booking cancelled",
                notificationBody,
                booking.Id.ToString(),
                ReferenceType.Booking);

            if (notificationResult.IsError)
                return notificationResult.Errors;

            notification = notificationResult.Value;
        }

        await context.SaveChangesAsync(ct);

        if (notification is not null)
            await notificationService.SendAsync(notification, ct);

        return true;
    }
}
