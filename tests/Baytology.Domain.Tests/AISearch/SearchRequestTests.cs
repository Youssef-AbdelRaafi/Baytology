using Baytology.Domain.AISearch;
using Baytology.Domain.AISearch.Events;
using Baytology.Domain.Common.Enums;

namespace Baytology.Domain.Tests.AISearch;

public sealed class SearchRequestTests
{
    [Fact]
    public void Create_starts_pending_and_raises_search_requested_event()
    {
        var request = SearchRequest.Create("buyer-1", SearchInputType.Text, SearchEngine.Hybrid, "corr-1").Value;

        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Single(request.DomainEvents);
        Assert.IsType<SearchRequestedEvent>(request.DomainEvents.Single());
    }

    [Fact]
    public void Complete_and_fail_update_status_and_resolution_time()
    {
        var completedRequest = SearchRequest.Create("buyer-1", SearchInputType.Text, SearchEngine.Hybrid).Value;
        completedRequest.Complete(7);

        Assert.Equal(RequestStatus.Completed, completedRequest.Status);
        Assert.Equal(7, completedRequest.ResultCount);
        Assert.NotNull(completedRequest.ResolvedAt);

        var failedRequest = SearchRequest.Create("buyer-1", SearchInputType.Text, SearchEngine.Hybrid).Value;
        failedRequest.Fail();

        Assert.Equal(RequestStatus.Failed, failedRequest.Status);
        Assert.NotNull(failedRequest.ResolvedAt);
    }

    [Fact]
    public void Create_returns_error_when_user_is_missing()
    {
        var result = SearchRequest.Create(string.Empty, SearchInputType.Text, SearchEngine.Hybrid);

        Assert.True(result.IsError);
        Assert.Equal("SearchRequest_User_Required", result.TopError.Code);
    }
}
