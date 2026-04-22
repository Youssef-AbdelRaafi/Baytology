using FluentValidation;

namespace Baytology.Application.Features.Identity.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
