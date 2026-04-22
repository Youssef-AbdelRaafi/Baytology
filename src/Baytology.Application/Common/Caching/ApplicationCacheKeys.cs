using System.Globalization;

namespace Baytology.Application.Common.Caching;

public static class ApplicationCacheKeys
{
    public static string Properties(
        string? city,
        string? district,
        string? propertyType,
        string? listingType,
        decimal? minPrice,
        decimal? maxPrice,
        decimal? minArea,
        decimal? maxArea,
        int? minBedrooms,
        int? maxBedrooms,
        int pageNumber,
        int pageSize,
        string? agentUserId = null)
        => string.Join('|',
        [
            "properties",
            Normalize(city),
            Normalize(district),
            Normalize(propertyType),
            Normalize(listingType),
            Normalize(minPrice),
            Normalize(maxPrice),
            Normalize(minArea),
            Normalize(maxArea),
            Normalize(minBedrooms),
            Normalize(maxBedrooms),
            pageNumber.ToString(CultureInfo.InvariantCulture),
            pageSize.ToString(CultureInfo.InvariantCulture),
            Normalize(agentUserId)
        ]);

    public static string Property(Guid propertyId)
        => $"property:{propertyId:N}";

    public static string SavedProperties(string userId, int pageNumber, int pageSize)
        => $"saved-properties:{Normalize(userId)}:{pageNumber}:{pageSize}";

    public static string AgentDetail(string userId)
        => $"agent-detail:{Normalize(userId)}";

    public static string UserProfile(string userId)
        => $"user-profile:{Normalize(userId)}";

    public static string SearchRequest(Guid searchRequestId, string userId)
        => $"search-request:{searchRequestId:N}:{Normalize(userId)}";

    public static string RecommendationRequest(Guid recommendationRequestId, string userId)
        => $"recommendation-request:{recommendationRequestId:N}:{Normalize(userId)}";

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? "_"
            : value.Trim().ToLowerInvariant();

    private static string Normalize(decimal? value)
        => value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : "_";

    private static string Normalize(int? value)
        => value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : "_";
}
