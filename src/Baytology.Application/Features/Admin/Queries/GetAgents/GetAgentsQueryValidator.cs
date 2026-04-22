using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetAgents;

public sealed class GetAgentsQueryValidator : AbstractValidator<GetAgentsQuery>
{
    public GetAgentsQueryValidator()
    {
    }
}
