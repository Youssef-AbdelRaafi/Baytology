namespace Baytology.Contracts.Responses.Recommendations;

public sealed record RecommendationRequestResponse(
    Guid Id,
    string RequestedByUserId,
    string SourceEntityType,
    string? SourceEntityId,
    int TopN,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyCollection<RecommendationResultResponse> Results);
