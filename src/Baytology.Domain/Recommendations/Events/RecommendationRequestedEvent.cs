using Baytology.Domain.Common;

namespace Baytology.Domain.Recommendations.Events;

public sealed class RecommendationRequestedEvent(
    Guid requestId,
    string userId,
    string sourceEntityType,
    string? sourceEntityId,
    int topN,
    string? correlationId) : DomainEvent
{
    public Guid RequestId { get; } = requestId;
    public string UserId { get; } = userId;
    public string SourceEntityType { get; } = sourceEntityType;
    public string? SourceEntityId { get; } = sourceEntityId;
    public int TopN { get; } = topN;
    public string? CorrelationId { get; } = correlationId;
}
