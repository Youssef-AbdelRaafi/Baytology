using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;

namespace Baytology.Domain.AISearch.Events;

public sealed class SearchRequestedEvent(
    Guid searchRequestId,
    string userId,
    SearchInputType inputType,
    SearchEngine searchEngine,
    string? correlationId) : DomainEvent
{
    public Guid SearchRequestId { get; } = searchRequestId;
    public string UserId { get; } = userId;
    public SearchInputType InputType { get; } = inputType;
    public SearchEngine SearchEngine { get; } = searchEngine;
    public string? CorrelationId { get; } = correlationId;
}
