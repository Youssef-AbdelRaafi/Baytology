using FluentValidation;

namespace Baytology.Application.Features.Admin.Commands.VerifyAgent;

public class VerifyAgentCommandValidator : AbstractValidator<VerifyAgentCommand>
{
    public VerifyAgentCommandValidator()
    {
        RuleFor(x => x.AgentUserId).NotEmpty();
    }
}
