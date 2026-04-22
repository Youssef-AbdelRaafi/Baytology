namespace Baytology.Contracts.Requests.Properties;

public sealed record CreateAgentReviewRequest(
    string AgentUserId,
    Guid? PropertyId,
    int Rating,
    string? Comment);
