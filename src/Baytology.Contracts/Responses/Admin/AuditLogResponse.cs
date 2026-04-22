namespace Baytology.Contracts.Responses.Admin;

public sealed record AuditLogResponse(
    Guid Id,
    string? UserId,
    string Action,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTimeOffset OccurredOnUtc);
