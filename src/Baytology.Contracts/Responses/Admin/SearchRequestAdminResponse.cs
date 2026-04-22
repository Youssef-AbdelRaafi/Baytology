namespace Baytology.Contracts.Responses.Admin;

public sealed record SearchRequestAdminResponse(
    Guid Id,
    string UserId,
    string InputType,
    string SearchEngine,
    string Status,
    int ResultCount,
    string? CorrelationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    int OutboxEventCount);
