namespace Baytology.Contracts.Requests.Recommendations;

public sealed record CreateRecommendationRequest(
    string SourceEntityType,
    string? SourceEntityId,
    int TopN = 10);
