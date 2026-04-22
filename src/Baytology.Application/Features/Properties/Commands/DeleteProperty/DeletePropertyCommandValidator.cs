using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.DeleteProperty;

public class DeletePropertyCommandValidator : AbstractValidator<DeletePropertyCommand>
{
    public DeletePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.AgentUserId).NotEmpty();
    }
}
