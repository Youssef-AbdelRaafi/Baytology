namespace Baytology.Application.Common.Caching;

public static class ApplicationCacheTags
{
    public const string Properties = "properties";
    public const string SavedProperties = "saved-properties";
    public const string AgentDetails = "agent-details";
    public const string UserProfiles = "user-profiles";
    public const string SearchRequests = "search-requests";
    public const string RecommendationRequests = "recommendation-requests";

    public static string Property(Guid propertyId)
        => $"property:{propertyId:N}";

    public static string SavedPropertiesByUser(string userId)
        => $"saved-properties:user:{Normalize(userId)}";

    public static string AgentDetail(string userId)
        => $"agent-detail:user:{Normalize(userId)}";

    public static string UserProfile(string userId)
        => $"user-profile:{Normalize(userId)}";

    public static string SearchRequest(Guid searchRequestId)
        => $"search-request:{searchRequestId:N}";

    public static string RecommendationRequest(Guid recommendationRequestId)
        => $"recommendation-request:{recommendationRequestId:N}";

    private static string Normalize(string value)
        => value.Trim().ToLowerInvariant();
}
