using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AgentDetails;

public static class AgentDetailErrors
{
    public static readonly Error UserIdRequired =
        Error.Validation("AgentDetail_UserId_Required", "User ID is required.");

    public static readonly Error NotFound =
        Error.NotFound("AgentDetail_Not_Found", "Agent details not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("AgentDetail_Already_Exists", "Agent details already exist for this user.");
}
