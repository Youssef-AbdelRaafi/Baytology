using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error PropertyRequired =
        Error.Validation("Payment_Property_Required", "Property ID is required.");

    public static readonly Error PaymentIdRequired =
        Error.Validation("Payment_Id_Required", "Payment ID is required.");

    public static readonly Error PayerRequired =
        Error.Validation("Payment_Payer_Required", "Payer ID is required.");

    public static readonly Error PayeeRequired =
        Error.Validation("Payment_Payee_Required", "Payee ID is required.");

    public static readonly Error AmountInvalid =
        Error.Validation("Payment_Amount_Invalid", "Payment amount must be greater than zero.");

    public static readonly Error CommissionRateInvalid =
        Error.Validation("Payment_CommissionRate_Invalid", "Commission rate must be between 0 and 1.");

    public static readonly Error CurrencyRequired =
        Error.Validation("Payment_Currency_Required", "Payment currency is required.");

    public static readonly Error TransactionGatewayRequired =
        Error.Validation("Payment_Transaction_Gateway_Required", "Gateway name is required.");

    public static readonly Error TransactionStatusRequired =
        Error.Validation("Payment_Transaction_Status_Required", "Transaction status is required.");

    public static readonly Error GatewayReferenceTooLong =
        Error.Validation("Payment_Transaction_GatewayReference_TooLong", "Gateway reference cannot exceed 200 characters.");

    public static readonly Error GatewayNameTooLong =
        Error.Validation("Payment_Transaction_GatewayName_TooLong", "Gateway name cannot exceed 50 characters.");

    public static readonly Error TransactionStatusTooLong =
        Error.Validation("Payment_Transaction_Status_TooLong", "Transaction status cannot exceed 50 characters.");

    public static readonly Error RefundRequestedByRequired =
        Error.Validation("Refund_RequestedBy_Required", "The requesting user is required.");

    public static readonly Error RefundReasonRequired =
        Error.Validation("Refund_Reason_Required", "Refund reason is required.");

    public static readonly Error RefundReasonTooLong =
        Error.Validation("Refund_Reason_TooLong", "Refund reason cannot exceed 2000 characters.");

    public static readonly Error RefundAmountInvalid =
        Error.Validation("Refund_Amount_Invalid", "Refund amount must be greater than zero.");

    public static readonly Error RefundReviewerRequired =
        Error.Validation("Refund_Reviewer_Required", "The reviewing user is required.");

    public static readonly Error RefundAlreadyReviewed =
        Error.Conflict("Refund_Already_Reviewed", "Refund request has already been reviewed.");

    public static readonly Error RefundNotApproved =
        Error.Conflict("Refund_Not_Approved", "Only approved refund requests can be marked as processed.");

    public static readonly Error NotFound =
        Error.NotFound("Payment_Not_Found", "Payment not found.");

    public static readonly Error AlreadyCompleted =
        Error.Conflict("Payment_Already_Completed", "Payment has already been completed.");

    public static readonly Error RefundNotFound =
        Error.NotFound("Refund_Not_Found", "Refund request not found.");
}
