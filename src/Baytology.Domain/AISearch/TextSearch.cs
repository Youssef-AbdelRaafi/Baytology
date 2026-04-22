using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public sealed class TextSearch : Entity
{
    public Guid SearchRequestId { get; private set; }
    public string RawQuery { get; private set; } = null!;
    public string? ParsedQuery { get; private set; }

    private TextSearch() { }

    private TextSearch(Guid searchRequestId, string rawQuery, string? parsedQuery = null)
        : base(Guid.NewGuid())
    {
        SearchRequestId = searchRequestId;
        RawQuery = rawQuery;
        ParsedQuery = parsedQuery;
    }

    public static Result<TextSearch> Create(Guid searchRequestId, string? rawQuery, string? parsedQuery = null)
    {
        if (searchRequestId == Guid.Empty)
            return SearchErrors.SearchRequestIdRequired;

        var normalizedRawQuery = rawQuery?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRawQuery))
            return SearchErrors.TextQueryRequired;

        if (normalizedRawQuery.Length > 2000)
            return SearchErrors.TextQueryTooLong;

        var normalizedParsedQuery = string.IsNullOrWhiteSpace(parsedQuery)
            ? null
            : parsedQuery.Trim();

        if (normalizedParsedQuery is not null && normalizedParsedQuery.Length > 2000)
            return SearchErrors.TextQueryTooLong;

        return new TextSearch(searchRequestId, normalizedRawQuery, normalizedParsedQuery);
    }
}
