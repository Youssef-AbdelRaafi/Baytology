using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.Payments.Commands.ProcessWebhook;

public class ProcessPaymentWebhookCommandHandler(
    IAppDbContext context,
    INotificationService notificationService,
    ILogger<ProcessPaymentWebhookCommandHandler> logger)
    : IRequestHandler<ProcessPaymentWebhookCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ProcessPaymentWebhookCommand request, CancellationToken ct)
    {
        var normalizedStatus = NormalizeStatus(request.TransactionStatus);
        Payment? payment = null;

        if (request.PaymentId.HasValue)
        {
            payment = await context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId.Value, ct);
        }

        if (payment is null && !string.IsNullOrWhiteSpace(request.GatewayReference))
        {
            var transaction = await context.PaymentTransactions
                .Where(t => t.GatewayReference == request.GatewayReference)
                .OrderByDescending(t => t.ProcessedAt)
                .FirstOrDefaultAsync(ct);

            if (transaction is not null)
            {
                payment = await context.Payments.FindAsync([transaction.PaymentId], ct);
            }
        }

        if (payment is null)
            return PaymentErrors.NotFound;

        var resolvedGatewayReference = ResolveGatewayReference(request.GatewayReference, payment.Id, normalizedStatus);

        var isDuplicateWebhook = await context.PaymentTransactions.AnyAsync(
            t => t.PaymentId == payment.Id &&
                 t.GatewayReference == resolvedGatewayReference &&
                 t.TransactionStatus == normalizedStatus,
            ct);

        if (isDuplicateWebhook)
            return true;

        var transactionResult = payment.RecordTransaction(
            resolvedGatewayReference,
            "Paymob",
            normalizedStatus,
            request.RawResponse);

        if (transactionResult.IsError)
            return transactionResult.Errors;

        context.PaymentTransactions.Add(transactionResult.Value);

        var shouldNotifyFailure = false;

        if (normalizedStatus == "success")
        {
            if (payment.Status != PaymentStatus.Refunded)
                payment.Complete();
        }
        else if (normalizedStatus is "failed" or "declined")
        {
            shouldNotifyFailure = payment.MarkFailed();
        }

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var duplicateCreatedByConcurrentRequest = await context.PaymentTransactions
                .AsNoTracking()
                .AnyAsync(
                    t => t.PaymentId == payment.Id &&
                         t.GatewayReference == resolvedGatewayReference &&
                         t.TransactionStatus == normalizedStatus,
                    ct);

            if (!duplicateCreatedByConcurrentRequest)
                throw;

            return true;
        }

        if (shouldNotifyFailure)
        {
            var notificationResult = Notification.Create(
                payment.PayerId,
                NotificationType.PaymentUpdate,
                "Payment failed",
                "The payment attempt was not completed successfully. Please try again.",
                payment.Id.ToString(),
                ReferenceType.Payment);

            if (notificationResult.IsError)
                return notificationResult.Errors;

            try
            {
                await notificationService.SendAsync(
                    notificationResult.Value,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to persist or deliver payment failure notification for payment {PaymentId}.",
                    payment.Id);
            }
        }

        return true;
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant();
    }

    private static string ResolveGatewayReference(string? gatewayReference, Guid paymentId, string normalizedStatus)
    {
        if (!string.IsNullOrWhiteSpace(gatewayReference))
            return gatewayReference.Trim();

        return $"payment:{paymentId:N}:{normalizedStatus}";
    }
}
