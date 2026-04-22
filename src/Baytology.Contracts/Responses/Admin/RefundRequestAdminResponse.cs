namespace Baytology.Contracts.Responses.Admin;

public sealed record RefundRequestAdminResponse(
    Guid Id,
    Guid PaymentId,
    string RequestedBy,
    string Reason,
    decimal Amount,
    string Status,
    string? ReviewedBy,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? ReviewedOnUtc);
