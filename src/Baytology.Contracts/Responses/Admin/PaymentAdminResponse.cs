namespace Baytology.Contracts.Responses.Admin;

public sealed record PaymentAdminResponse(
    Guid Id,
    Guid PropertyId,
    string PropertyTitle,
    string PayerId,
    string PayeeId,
    decimal Amount,
    decimal Commission,
    decimal NetAmount,
    string Currency,
    string Purpose,
    string Status,
    string? LatestGatewayReference,
    string? LatestTransactionStatus,
    DateTimeOffset CreatedOnUtc);
