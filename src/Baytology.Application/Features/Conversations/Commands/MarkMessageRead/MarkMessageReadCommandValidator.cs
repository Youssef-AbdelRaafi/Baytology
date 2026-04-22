using FluentValidation;

namespace Baytology.Application.Features.Conversations.Commands.MarkMessageRead;

public class MarkMessageReadCommandValidator : AbstractValidator<MarkMessageReadCommand>
{
    public MarkMessageReadCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
