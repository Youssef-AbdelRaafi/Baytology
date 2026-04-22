using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.SaveProperty;

public class SavePropertyCommandValidator : AbstractValidator<SavePropertyCommand>
{
    public SavePropertyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
    }
}
