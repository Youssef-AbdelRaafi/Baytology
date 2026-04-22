using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Recommendations;

public static class RecommendationErrors
{
    public static readonly Error RequestedByRequired =
        Error.Validation("RecommendationRequest_User_Required", "Recommendation request user is required.");

    public static readonly Error SourceEntityTypeRequired =
        Error.Validation("RecommendationRequest_SourceEntityType_Required", "Source entity type is required.");

    public static readonly Error SourceEntityTypeTooLong =
        Error.Validation("RecommendationRequest_SourceEntityType_TooLong", "Source entity type cannot exceed 50 characters.");

    public static readonly Error SourceEntityIdTooLong =
        Error.Validation("RecommendationRequest_SourceEntityId_TooLong", "Source entity id cannot exceed 200 characters.");

    public static readonly Error TopNInvalid =
        Error.Validation("RecommendationRequest_TopN_Invalid", "TopN must be between 1 and 50.");

    public static readonly Error CorrelationIdTooLong =
        Error.Validation("RecommendationRequest_CorrelationId_TooLong", "Correlation id cannot exceed 200 characters.");

    public static readonly Error ModelVersionTooLong =
        Error.Validation("RecommendationRequest_ModelVersion_TooLong", "Model version cannot exceed 50 characters.");

    public static readonly Error RequestIdRequired =
        Error.Validation("RecommendationRequest_Id_Required", "Recommendation request id is required.");

    public static readonly Error ReferenceRequired =
        Error.Validation("RecommendationResult_Reference_Required", "Each recommendation result must include a property id or an external reference.");

    public static readonly Error RankInvalid =
        Error.Validation("RecommendationResult_Rank_Invalid", "Rank must be greater than zero.");

    public static readonly Error SimilarityScoreInvalid =
        Error.Validation("RecommendationResult_SimilarityScore_Invalid", "Similarity score cannot be negative.");

    public static readonly Error ExternalReferenceTooLong =
        Error.Validation("RecommendationResult_ExternalReference_TooLong", "External reference cannot exceed 500 characters.");

    public static readonly Error SnapshotTitleTooLong =
        Error.Validation("RecommendationResult_SnapshotTitle_TooLong", "Snapshot title cannot exceed 500 characters.");

    public static readonly Error SnapshotPriceInvalid =
        Error.Validation("RecommendationResult_SnapshotPrice_Invalid", "Snapshot price cannot be negative.");
}
