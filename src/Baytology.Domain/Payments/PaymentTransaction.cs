using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Payments;

public sealed class PaymentTransaction : Entity
{
    public Guid PaymentId { get; private set; }
    public string? GatewayReference { get; private set; }
    public string? GatewayName { get; private set; }
    public string? TransactionStatus { get; private set; }
    public string? RawResponse { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    private PaymentTransaction() { }

    private PaymentTransaction(
        Guid paymentId,
        string? gatewayReference,
        string gatewayName,
        string transactionStatus,
        string? rawResponse) : base(Guid.NewGuid())
    {
        PaymentId = paymentId;
        GatewayReference = gatewayReference;
        GatewayName = gatewayName;
        TransactionStatus = transactionStatus;
        RawResponse = rawResponse;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public static Result<PaymentTransaction> Create(
        Guid paymentId,
        string? gatewayReference,
        string gatewayName,
        string transactionStatus,
        string? rawResponse)
    {
        if (paymentId == Guid.Empty)
            return PaymentErrors.PaymentIdRequired;

        var normalizedGatewayReference = string.IsNullOrWhiteSpace(gatewayReference)
            ? null
            : gatewayReference.Trim();

        var normalizedGatewayName = gatewayName?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedGatewayName))
            return PaymentErrors.TransactionGatewayRequired;

        if (normalizedGatewayName.Length > 50)
            return PaymentErrors.GatewayNameTooLong;

        var normalizedStatus = transactionStatus?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedStatus))
            return PaymentErrors.TransactionStatusRequired;

        if (normalizedStatus.Length > 50)
            return PaymentErrors.TransactionStatusTooLong;

        if (normalizedGatewayReference is not null && normalizedGatewayReference.Length > 200)
            return PaymentErrors.GatewayReferenceTooLong;

        return new PaymentTransaction(paymentId, normalizedGatewayReference, normalizedGatewayName, normalizedStatus, rawResponse);
    }
}
