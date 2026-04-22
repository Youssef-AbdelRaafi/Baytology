using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Payments.Events;

namespace Baytology.Domain.Payments;

public sealed class Payment : AuditableEntity
{
    public Guid PropertyId { get; private set; }
    public string PayerId { get; private set; } = null!;
    public string PayeeId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public decimal Commission { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public PaymentPurpose Purpose { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset? EscrowReleasedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PaymentTransaction> _transactions = [];
    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    private Payment() { }

    private Payment(
        Guid id,
        Guid propertyId,
        string payerId,
        string payeeId,
        decimal amount,
        decimal commission,
        PaymentPurpose purpose,
        string currency) : base(id)
    {
        PropertyId = propertyId;
        PayerId = payerId;
        PayeeId = payeeId;
        Amount = amount;
        Commission = commission;
        NetAmount = amount - commission;
        Purpose = purpose;
        Currency = currency;
        Status = PaymentStatus.Pending;
    }

    public static Result<Payment> Create(
        Guid propertyId,
        string payerId,
        string payeeId,
        decimal amount,
        decimal commissionRate,
        PaymentPurpose purpose,
        string currency = "EGP")
    {
        if (propertyId == Guid.Empty)
            return PaymentErrors.PropertyRequired;

        if (string.IsNullOrWhiteSpace(payerId))
            return PaymentErrors.PayerRequired;

        if (string.IsNullOrWhiteSpace(payeeId))
            return PaymentErrors.PayeeRequired;

        if (amount <= 0)
            return PaymentErrors.AmountInvalid;

        if (commissionRate < 0 || commissionRate >= 1)
            return PaymentErrors.CommissionRateInvalid;

        if (string.IsNullOrWhiteSpace(currency))
            return PaymentErrors.CurrencyRequired;

        var commission = amount * commissionRate;

        return new Payment(Guid.NewGuid(), propertyId, payerId, payeeId, amount, commission, purpose, currency);
    }

    public void MarkAsEscrow()
    {
        if (Status != PaymentStatus.Pending)
            return;

        Status = PaymentStatus.Escrow;
    }

    public bool Complete()
    {
        if (Status is PaymentStatus.Completed or PaymentStatus.Refunded)
            return false;

        Status = PaymentStatus.Completed;
        AddDomainEvent(new PaymentCompletedEvent(Id, PropertyId, PayerId));
        return true;
    }

    public bool ReleaseEscrow()
    {
        if (Status != PaymentStatus.Escrow)
            return false;

        Status = PaymentStatus.Completed;
        EscrowReleasedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PaymentCompletedEvent(Id, PropertyId, PayerId));
        return true;
    }

    public bool MarkFailed()
    {
        if (Status is PaymentStatus.Completed or PaymentStatus.Refunded or PaymentStatus.Failed)
            return false;

        Status = PaymentStatus.Failed;
        return true;
    }

    public bool MarkRefunded()
    {
        if (Status != PaymentStatus.Completed)
            return false;

        Status = PaymentStatus.Refunded;
        return true;
    }

    public Result<PaymentTransaction> RecordTransaction(
        string? gatewayReference,
        string gatewayName,
        string transactionStatus,
        string? rawResponse)
    {
        var transactionResult = PaymentTransaction.Create(Id, gatewayReference, gatewayName, transactionStatus, rawResponse);
        if (transactionResult.IsError)
            return transactionResult.Errors;

        _transactions.Add(transactionResult.Value);
        return transactionResult.Value;
    }
}
