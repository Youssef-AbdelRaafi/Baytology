using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public sealed class AgentReview : Entity
{
    public string AgentUserId { get; private set; } = null!;
    public string ReviewerUserId { get; private set; } = null!;
    public Guid? PropertyId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }

    private AgentReview() { }

    private AgentReview(
        Guid id, string agentUserId, string reviewerUserId,
        Guid? propertyId, int rating, string? comment) : base(id)
    {
        AgentUserId = agentUserId;
        ReviewerUserId = reviewerUserId;
        PropertyId = propertyId;
        Rating = rating;
        Comment = comment;
        CreatedOnUtc = DateTimeOffset.UtcNow;
    }

    public static Result<AgentReview> Create(
        string agentUserId, string reviewerUserId,
        Guid? propertyId, int rating, string? comment)
    {
        if (string.IsNullOrWhiteSpace(agentUserId))
            return AgentReviewErrors.AgentRequired;

        if (string.IsNullOrWhiteSpace(reviewerUserId))
            return AgentReviewErrors.ReviewerRequired;

        if (string.Equals(agentUserId, reviewerUserId, StringComparison.Ordinal))
            return AgentReviewErrors.SelfReviewNotAllowed;

        if (rating < 1 || rating > 5)
            return AgentReviewErrors.RatingInvalid;

        return new AgentReview(Guid.NewGuid(), agentUserId, reviewerUserId, propertyId, rating, comment);
    }
}
