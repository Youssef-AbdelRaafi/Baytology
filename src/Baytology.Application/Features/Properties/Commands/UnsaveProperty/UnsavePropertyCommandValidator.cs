using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.UnsaveProperty;

public class UnsavePropertyCommandValidator : AbstractValidator<UnsavePropertyCommand>
{
    public UnsavePropertyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
    }
}
