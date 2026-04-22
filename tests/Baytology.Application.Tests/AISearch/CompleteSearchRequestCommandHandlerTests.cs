using Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;
using Baytology.Application.Tests.Support;
using Baytology.Domain.AISearch;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Properties;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Tests.AISearch;

public sealed class CompleteSearchRequestCommandHandlerTests
{
    [Fact]
    public async Task Completing_search_request_persists_ranked_results_with_property_snapshots()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CompleteSearchRequestCommandHandler(context);

        var property = Property.Create(
            "agent-1",
            "Snapshot Apartment",
            "Description",
            PropertyType.Apartment,
            ListingType.Rent,
            14000m,
            150m,
            3,
            2,
            "Cairo",
            "Nasr City").Value;
        var searchRequest = SearchRequest.Create("buyer-1", SearchInputType.Text, SearchEngine.Hybrid, "corr-search").Value;

        context.Properties.Add(property);
        context.SearchRequests.Add(searchRequest);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new CompleteSearchRequestCommand(
                searchRequest.Id,
                true,
                [
                    new CompleteSearchResultInput(
                        property.Id,
                        1,
                        0.97f,
                        "hybrid",
                        null,
                        null,
                        null,
                        null)
                ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedRequest = await context.SearchRequests.FindAsync(searchRequest.Id);
        var savedResult = await context.SearchResults.SingleAsync(r => r.SearchRequestId == searchRequest.Id);

        Assert.NotNull(savedRequest);
        Assert.Equal(RequestStatus.Completed, savedRequest!.Status);
        Assert.Equal(1, savedRequest.ResultCount);
        Assert.Equal("Snapshot Apartment", savedResult.SnapshotTitle);
        Assert.Equal(14000m, savedResult.SnapshotPrice);
        Assert.Equal("Cairo", savedResult.SnapshotCity);
        Assert.Equal(PropertyStatus.Available.ToString(), savedResult.SnapshotStatus);
    }
}
