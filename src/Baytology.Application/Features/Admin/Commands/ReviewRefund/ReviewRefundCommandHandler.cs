using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Admin.Commands.ReviewRefund;

public class ReviewRefundCommandHandler(IAppDbContext context, INotificationService notificationService)
    : IRequestHandler<ReviewRefundCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ReviewRefundCommand request, CancellationToken ct)
    {
        var refund = await context.RefundRequests.FirstOrDefaultAsync(r => r.Id == request.RefundId, ct);
        if (refund is null) return ApplicationErrors.Admin.RefundNotFound;

        if (refund.Status != RefundStatus.Pending)
            return ApplicationErrors.Admin.RefundAlreadyReviewed;

        var payment = await context.Payments.FirstOrDefaultAsync(p => p.Id == refund.PaymentId, ct);
        if (payment is null)
            return PaymentErrors.NotFound;

        if (request.Approve)
        {
            if (refund.Amount != payment.Amount)
            {
                return ApplicationErrors.Admin.PartialRefundNotSupported;
            }

            if (!payment.MarkRefunded())
                return ApplicationErrors.Admin.PaymentNotRefundable;

            var approveResult = refund.Approve(request.AdminUserId);
            if (approveResult.IsError)
                return approveResult.Errors;

            var processResult = refund.MarkProcessed();
            if (processResult.IsError)
                return processResult.Errors;
        }
        else
        {
            var rejectResult = refund.Reject(request.AdminUserId);
            if (rejectResult.IsError)
                return rejectResult.Errors;
        }

        await context.SaveChangesAsync(ct);

        var notificationResult = Notification.Create(
            payment.PayerId,
            NotificationType.PaymentUpdate,
            request.Approve ? "Refund approved" : "Refund rejected",
            request.Approve
                ? "Your refund request was approved and processed successfully."
                : "Your refund request was rejected by the administrator.",
            refund.Id.ToString(),
            ReferenceType.Payment);

        if (notificationResult.IsError)
            return notificationResult.Errors;

        await notificationService.SendAsync(
            notificationResult.Value,
            ct);

        return Result.Success;
    }
}
