using Baytology.Domain.Common.Enums;
using Baytology.Domain.Recommendations;
using Baytology.Domain.Recommendations.Events;

namespace Baytology.Domain.Tests.Recommendations;

public sealed class RecommendationRequestTests
{
    [Fact]
    public void Create_starts_pending_and_raises_recommendation_requested_event()
    {
        var request = RecommendationRequest.Create(
            "buyer-1",
            "Property",
            Guid.NewGuid().ToString(),
            5,
            "corr-1",
            "v1").Value;

        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Single(request.DomainEvents);
        Assert.IsType<RecommendationRequestedEvent>(request.DomainEvents.Single());
    }

    [Fact]
    public void Complete_and_fail_update_status_and_resolution_time()
    {
        var completedRequest = RecommendationRequest.Create("buyer-1", "Property", null, 5).Value;
        completedRequest.Complete();

        Assert.Equal(RequestStatus.Completed, completedRequest.Status);
        Assert.NotNull(completedRequest.ResolvedAt);

        var failedRequest = RecommendationRequest.Create("buyer-1", "Property", null, 5).Value;
        failedRequest.Fail();

        Assert.Equal(RequestStatus.Failed, failedRequest.Status);
        Assert.NotNull(failedRequest.ResolvedAt);
    }

    [Fact]
    public void Create_returns_error_when_topn_is_invalid()
    {
        var result = RecommendationRequest.Create("buyer-1", "Property", null, 0);

        Assert.True(result.IsError);
        Assert.Equal("RecommendationRequest_TopN_Invalid", result.TopError.Code);
    }
}
