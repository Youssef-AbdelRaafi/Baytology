using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AISearch;

public static class SearchErrors
{
    public static readonly Error UserRequired =
        Error.Validation("SearchRequest_User_Required", "Search request user is required.");

    public static readonly Error CorrelationIdTooLong =
        Error.Validation("SearchRequest_CorrelationId_TooLong", "Correlation id cannot exceed 200 characters.");

    public static readonly Error SearchRequestIdRequired =
        Error.Validation("SearchRequest_Id_Required", "Search request id is required.");

    public static readonly Error TextQueryRequired =
        Error.Validation("SearchRequest_TextQuery_Required", "Text search query is required.");

    public static readonly Error TextQueryTooLong =
        Error.Validation("SearchRequest_TextQuery_TooLong", "Text search query cannot exceed 2000 characters.");

    public static readonly Error AudioFileUrlRequired =
        Error.Validation("SearchRequest_AudioFileUrl_Required", "Audio file url is required.");

    public static readonly Error AudioFileUrlTooLong =
        Error.Validation("SearchRequest_AudioFileUrl_TooLong", "Audio file url cannot exceed 1000 characters.");

    public static readonly Error ImageFileUrlRequired =
        Error.Validation("SearchRequest_ImageFileUrl_Required", "Image file url is required.");

    public static readonly Error ImageFileUrlTooLong =
        Error.Validation("SearchRequest_ImageFileUrl_TooLong", "Image file url cannot exceed 1000 characters.");

    public static readonly Error LanguageTooLong =
        Error.Validation("SearchRequest_Language_TooLong", "Language cannot exceed 10 characters.");

    public static readonly Error MinPriceInvalid =
        Error.Validation("SearchFilter_MinPrice_Invalid", "Minimum price cannot be negative.");

    public static readonly Error MaxPriceInvalid =
        Error.Validation("SearchFilter_MaxPrice_Invalid", "Maximum price cannot be negative.");

    public static readonly Error PriceRangeInvalid =
        Error.Validation("SearchFilter_PriceRange_Invalid", "Minimum price cannot exceed maximum price.");

    public static readonly Error MinAreaInvalid =
        Error.Validation("SearchFilter_MinArea_Invalid", "Minimum area cannot be negative.");

    public static readonly Error MaxAreaInvalid =
        Error.Validation("SearchFilter_MaxArea_Invalid", "Maximum area cannot be negative.");

    public static readonly Error AreaRangeInvalid =
        Error.Validation("SearchFilter_AreaRange_Invalid", "Minimum area cannot exceed maximum area.");

    public static readonly Error MinBedroomsInvalid =
        Error.Validation("SearchFilter_MinBedrooms_Invalid", "Minimum bedrooms cannot be negative.");

    public static readonly Error MaxBedroomsInvalid =
        Error.Validation("SearchFilter_MaxBedrooms_Invalid", "Maximum bedrooms cannot be negative.");

    public static readonly Error BedroomRangeInvalid =
        Error.Validation("SearchFilter_BedroomRange_Invalid", "Minimum bedrooms cannot exceed maximum bedrooms.");

    public static readonly Error PropertyIdRequired =
        Error.Validation("SearchResult_PropertyId_Required", "Property id is required.");

    public static readonly Error RankInvalid =
        Error.Validation("SearchResult_Rank_Invalid", "Rank must be greater than zero.");

    public static readonly Error RelevanceScoreInvalid =
        Error.Validation("SearchResult_RelevanceScore_Invalid", "Relevance score cannot be negative.");

    public static readonly Error ScoreSourceTooLong =
        Error.Validation("SearchResult_ScoreSource_TooLong", "Score source cannot exceed 50 characters.");

    public static readonly Error SnapshotTitleTooLong =
        Error.Validation("SearchResult_SnapshotTitle_TooLong", "Snapshot title cannot exceed 500 characters.");

    public static readonly Error SnapshotPriceInvalid =
        Error.Validation("SearchResult_SnapshotPrice_Invalid", "Snapshot price cannot be negative.");

    public static readonly Error SnapshotCityTooLong =
        Error.Validation("SearchResult_SnapshotCity_TooLong", "Snapshot city cannot exceed 100 characters.");

    public static readonly Error SnapshotStatusTooLong =
        Error.Validation("SearchResult_SnapshotStatus_TooLong", "Snapshot status cannot exceed 30 characters.");
}
