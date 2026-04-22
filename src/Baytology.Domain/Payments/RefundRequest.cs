using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Payments;

public sealed class RefundRequest : Entity
{
    public Guid PaymentId { get; private set; }
    public string RequestedBy { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public RefundStatus Status { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public DateTimeOffset? ReviewedOnUtc { get; private set; }

    private RefundRequest() { }

    private RefundRequest(Guid paymentId, string requestedBy, string reason, decimal amount)
        : base(Guid.NewGuid())
    {
        PaymentId = paymentId;
        RequestedBy = requestedBy;
        Reason = reason;
        Amount = amount;
        Status = RefundStatus.Pending;
        CreatedOnUtc = DateTimeOffset.UtcNow;
    }

    public static Result<RefundRequest> Create(Guid paymentId, string requestedBy, string reason, decimal amount)
    {
        if (paymentId == Guid.Empty)
            return PaymentErrors.PaymentIdRequired;

        if (string.IsNullOrWhiteSpace(requestedBy))
            return PaymentErrors.RefundRequestedByRequired;

        var normalizedReason = reason?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            return PaymentErrors.RefundReasonRequired;

        if (normalizedReason.Length > 2000)
            return PaymentErrors.RefundReasonTooLong;

        if (amount <= 0)
            return PaymentErrors.RefundAmountInvalid;

        return new RefundRequest(paymentId, requestedBy.Trim(), normalizedReason, amount);
    }

    public Result<Success> Approve(string reviewedBy)
    {
        var reviewState = BeginReview(reviewedBy);
        if (reviewState.IsError)
            return reviewState.Errors;

        Status = RefundStatus.Approved;
        return Result.Success;
    }

    public Result<Success> Reject(string reviewedBy)
    {
        var reviewState = BeginReview(reviewedBy);
        if (reviewState.IsError)
            return reviewState.Errors;

        Status = RefundStatus.Rejected;
        return Result.Success;
    }

    public Result<Success> MarkProcessed()
    {
        if (Status != RefundStatus.Approved)
            return PaymentErrors.RefundNotApproved;

        Status = RefundStatus.Processed;
        return Result.Success;
    }

    private Result<Success> BeginReview(string reviewedBy)
    {
        if (Status != RefundStatus.Pending)
            return PaymentErrors.RefundAlreadyReviewed;

        if (string.IsNullOrWhiteSpace(reviewedBy))
            return PaymentErrors.RefundReviewerRequired;

        ReviewedBy = reviewedBy.Trim();
        ReviewedOnUtc = DateTimeOffset.UtcNow;
        return Result.Success;
    }
}
