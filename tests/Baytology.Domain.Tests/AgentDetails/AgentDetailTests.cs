using Baytology.Domain.AgentDetails;

namespace Baytology.Domain.Tests.AgentDetails;

public sealed class AgentDetailTests
{
    [Fact]
    public void Create_returns_validation_error_when_user_id_is_missing()
    {
        var result = AgentDetail.Create("");

        Assert.True(result.IsError);
        Assert.Equal(AgentDetailErrors.UserIdRequired, result.TopError);
    }

    [Fact]
    public void Verify_and_update_rating_apply_expected_changes()
    {
        var agentDetail = AgentDetail.Create("agent-1", "Baytology Estates", "LIC-1", 0.04m).Value;

        agentDetail.Verify();
        agentDetail.UpdateRating(4.8m, 12);

        Assert.True(agentDetail.IsVerified);
        Assert.Equal(4.8m, agentDetail.Rating);
        Assert.Equal(12, agentDetail.ReviewCount);
    }
}
