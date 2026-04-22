using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.AISearch.Events;

namespace Baytology.Domain.AISearch;

public sealed class SearchRequest : Entity
{
    public string UserId { get; private set; } = null!;
    public SearchInputType InputType { get; private set; }
    public SearchEngine SearchEngine { get; private set; }
    public string? CorrelationId { get; private set; }
    public RequestStatus Status { get; private set; }
    public int ResultCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    // Navigation
    public TextSearch? TextSearch { get; private set; }
    public VoiceSearch? VoiceSearch { get; private set; }
    public ImageSearch? ImageSearch { get; private set; }
    public SearchFilter? Filter { get; private set; }

    private readonly List<SearchResult> _results = [];
    public IReadOnlyCollection<SearchResult> Results => _results.AsReadOnly();

    private SearchRequest() { }

    private SearchRequest(string userId, SearchInputType inputType, SearchEngine searchEngine, string? correlationId = null)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        InputType = inputType;
        SearchEngine = searchEngine;
        CorrelationId = correlationId;
        Status = RequestStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new SearchRequestedEvent(Id, userId, inputType, searchEngine, correlationId));
    }

    public static Result<SearchRequest> Create(
        string userId,
        SearchInputType inputType,
        SearchEngine searchEngine,
        string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return SearchErrors.UserRequired;

        var normalizedCorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? null
            : correlationId.Trim();

        if (normalizedCorrelationId is not null && normalizedCorrelationId.Length > 200)
            return SearchErrors.CorrelationIdTooLong;

        return new SearchRequest(userId.Trim(), inputType, searchEngine, normalizedCorrelationId);
    }

    public void Complete(int resultCount)
    {
        Status = RequestStatus.Completed;
        ResultCount = resultCount;
        ResolvedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = RequestStatus.Failed;
        ResolvedAt = DateTimeOffset.UtcNow;
    }
}
