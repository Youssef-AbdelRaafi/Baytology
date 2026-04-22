namespace Baytology.Contracts.Responses.Admin;

public sealed record RecommendationRequestAdminResponse(
    Guid Id,
    string RequestedByUserId,
    string SourceEntityType,
    string? SourceEntityId,
    int TopN,
    string Status,
    string? CorrelationId,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ResolvedAt,
    int OutboxEventCount);
