using FluentValidation;

namespace Baytology.Application.Features.Admin.Commands.AssignRole;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.Role)
            .Must(role => role is "Buyer" or "Agent" or "Admin")
            .WithMessage("Role must be Buyer, Agent, or Admin.");
    }
}
