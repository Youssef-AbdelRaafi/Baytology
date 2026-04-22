using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Payments;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Payments.Commands.RequestRefund;

public class RequestRefundCommandHandler(IAppDbContext context)
    : IRequestHandler<RequestRefundCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RequestRefundCommand request, CancellationToken ct)
    {
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.PayerId == request.RequestedBy, ct);

        if (payment is null)
            return ApplicationErrors.Payment.NotFound;

        if (payment.Status != Domain.Common.Enums.PaymentStatus.Completed)
            return ApplicationErrors.Refund.PaymentNotCompleted;

        if (request.Amount > payment.Amount)
            return ApplicationErrors.Refund.AmountInvalid;

        if (request.Amount != payment.Amount)
        {
            return ApplicationErrors.Refund.AmountMustMatchPayment;
        }

        var hasPendingRefund = await context.RefundRequests.AnyAsync(
            r => r.PaymentId == request.PaymentId && r.Status == Domain.Common.Enums.RefundStatus.Pending,
            ct);

        if (hasPendingRefund)
            return ApplicationErrors.Refund.PendingExists;

        var refundResult = RefundRequest.Create(request.PaymentId, request.RequestedBy, request.Reason, request.Amount);
        if (refundResult.IsError)
            return refundResult.Errors;

        var refund = refundResult.Value;
        context.RefundRequests.Add(refund);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var duplicatePendingRefund = await context.RefundRequests.AnyAsync(
                r => r.PaymentId == request.PaymentId && r.Status == Domain.Common.Enums.RefundStatus.Pending,
                ct);

            if (duplicatePendingRefund)
                return ApplicationErrors.Refund.PendingExists;

            throw;
        }

        return refund.Id;
    }
}
