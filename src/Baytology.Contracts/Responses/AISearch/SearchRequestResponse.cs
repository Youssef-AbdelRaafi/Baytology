namespace Baytology.Contracts.Responses.AISearch;

public sealed record SearchRequestResponse(
    Guid Id,
    string UserId,
    string InputType,
    string SearchEngine,
    string Status,
    int ResultCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    IReadOnlyCollection<SearchResultResponse> Results);
