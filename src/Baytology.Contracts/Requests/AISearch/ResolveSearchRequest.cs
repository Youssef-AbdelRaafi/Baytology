namespace Baytology.Contracts.Requests.AISearch;

public sealed record ResolveSearchRequest(
    bool IsSuccessful,
    List<ResolveSearchResultRequest>? Results);
