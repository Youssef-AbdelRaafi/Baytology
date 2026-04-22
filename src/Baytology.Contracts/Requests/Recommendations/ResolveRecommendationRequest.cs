namespace Baytology.Contracts.Requests.Recommendations;

public sealed record ResolveRecommendationRequest(
    bool IsSuccessful,
    List<ResolveRecommendationResultRequest>? Results);
