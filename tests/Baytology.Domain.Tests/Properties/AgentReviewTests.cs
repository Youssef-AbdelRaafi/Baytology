using Baytology.Domain.Properties;

namespace Baytology.Domain.Tests.Properties;

public sealed class AgentReviewTests
{
    [Fact]
    public void Create_rejects_self_reviews()
    {
        var result = AgentReview.Create("agent-1", "agent-1", null, 5, "Self review");

        Assert.True(result.IsError);
        Assert.Equal(AgentReviewErrors.SelfReviewNotAllowed.Code, result.TopError.Code);
    }
}
