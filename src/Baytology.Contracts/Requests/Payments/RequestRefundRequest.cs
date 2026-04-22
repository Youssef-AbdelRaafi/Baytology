namespace Baytology.Contracts.Requests.Payments;

public sealed record RequestRefundRequest(
    Guid PaymentId,
    string Reason,
    decimal Amount);
