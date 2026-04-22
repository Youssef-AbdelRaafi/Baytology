using FluentValidation;

namespace Baytology.Application.Features.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}
