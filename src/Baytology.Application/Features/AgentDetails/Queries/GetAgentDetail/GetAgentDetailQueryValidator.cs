using FluentValidation;

namespace Baytology.Application.Features.AgentDetails.Queries.GetAgentDetail;

public sealed class GetAgentDetailQueryValidator : AbstractValidator<GetAgentDetailQuery>
{
    public GetAgentDetailQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
