using Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Properties;
using Baytology.Domain.Recommendations;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Tests.Recommendations;

public sealed class CompleteRecommendationRequestCommandHandlerTests
{
    [Fact]
    public async Task Successful_completion_can_recover_failed_recommendation_request()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CompleteRecommendationRequestCommandHandler(context);

        var property = Property.Create(
            "agent-1",
            "Recovered Recommendation",
            "Description",
            PropertyType.Apartment,
            ListingType.Sale,
            1700000m,
            180m,
            4,
            3,
            "Giza",
            "Sheikh Zayed").Value;
        var recommendationRequest = RecommendationRequest.Create("buyer-1", "Property", property.Id.ToString(), 5, "corr-rec").Value;
        recommendationRequest.Fail();

        context.Properties.Add(property);
        context.RecommendationRequests.Add(recommendationRequest);
        await context.SaveChangesAsync();

        var result = await handler.Handle(
            new CompleteRecommendationRequestCommand(
                recommendationRequest.Id,
                true,
                [
                    new CompleteRecommendationResultInput(
                        property.Id,
                        null,
                        0.91f,
                        1,
                        null,
                        null)
                ]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedRequest = await context.RecommendationRequests.FindAsync(recommendationRequest.Id);
        var savedResult = await context.RecommendationResults.SingleAsync(r => r.RequestId == recommendationRequest.Id);

        Assert.NotNull(savedRequest);
        Assert.Equal(RequestStatus.Completed, savedRequest!.Status);
        Assert.Equal(property.Id, savedResult.RecommendedPropertyId);
        Assert.Equal("Recovered Recommendation", savedResult.SnapshotTitle);
        Assert.Equal(1700000m, savedResult.SnapshotPrice);
    }
}
