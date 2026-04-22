using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public static class AgentReviewErrors
{
    public static readonly Error AgentRequired =
        Error.Validation("Review_Agent_Required", "Agent user ID is required.");

    public static readonly Error ReviewerRequired =
        Error.Validation("Review_Reviewer_Required", "Reviewer user ID is required.");

    public static readonly Error SelfReviewNotAllowed =
        Error.Validation("Review_SelfReview_Not_Allowed", "You cannot review yourself.");

    public static readonly Error RatingInvalid =
        Error.Validation("Review_Rating_Invalid", "Rating must be between 1 and 5.");
}
