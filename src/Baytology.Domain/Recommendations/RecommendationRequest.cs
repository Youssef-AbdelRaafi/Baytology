using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Recommendations.Events;

namespace Baytology.Domain.Recommendations;

public sealed class RecommendationRequest : Entity
{
    public string RequestedByUserId { get; private set; } = null!;
    public string SourceEntityType { get; private set; } = null!;
    public string? SourceEntityId { get; private set; }
    public int TopN { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? ModelVersion { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    private readonly List<RecommendationResult> _results = [];
    public IReadOnlyCollection<RecommendationResult> Results => _results.AsReadOnly();

    private RecommendationRequest() { }

    private RecommendationRequest(
        string requestedByUserId,
        string sourceEntityType,
        string? sourceEntityId,
        int topN,
        string? correlationId = null,
        string? modelVersion = null) : base(Guid.NewGuid())
    {
        RequestedByUserId = requestedByUserId;
        SourceEntityType = sourceEntityType;
        SourceEntityId = sourceEntityId;
        TopN = topN;
        CorrelationId = correlationId;
        ModelVersion = modelVersion;
        Status = RequestStatus.Pending;
        RequestedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new RecommendationRequestedEvent(
            Id,
            requestedByUserId,
            sourceEntityType,
            sourceEntityId,
            topN,
            correlationId));
    }

    public static Result<RecommendationRequest> Create(
        string requestedByUserId,
        string sourceEntityType,
        string? sourceEntityId,
        int topN,
        string? correlationId = null,
        string? modelVersion = null)
    {
        if (string.IsNullOrWhiteSpace(requestedByUserId))
            return RecommendationErrors.RequestedByRequired;

        var normalizedSourceEntityType = sourceEntityType?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedSourceEntityType))
            return RecommendationErrors.SourceEntityTypeRequired;

        if (normalizedSourceEntityType.Length > 50)
            return RecommendationErrors.SourceEntityTypeTooLong;

        var normalizedSourceEntityId = string.IsNullOrWhiteSpace(sourceEntityId) ? null : sourceEntityId.Trim();
        if (normalizedSourceEntityId is not null && normalizedSourceEntityId.Length > 200)
            return RecommendationErrors.SourceEntityIdTooLong;

        if (topN is < 1 or > 50)
            return RecommendationErrors.TopNInvalid;

        var normalizedCorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        if (normalizedCorrelationId is not null && normalizedCorrelationId.Length > 200)
            return RecommendationErrors.CorrelationIdTooLong;

        var normalizedModelVersion = string.IsNullOrWhiteSpace(modelVersion) ? null : modelVersion.Trim();
        if (normalizedModelVersion is not null && normalizedModelVersion.Length > 50)
            return RecommendationErrors.ModelVersionTooLong;

        return new RecommendationRequest(
            requestedByUserId.Trim(),
            normalizedSourceEntityType,
            normalizedSourceEntityId,
            topN,
            normalizedCorrelationId,
            normalizedModelVersion);
    }

    public void Complete()
    {
        Status = RequestStatus.Completed;
        ResolvedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        Status = RequestStatus.Failed;
        ResolvedAt = DateTimeOffset.UtcNow;
    }
}
