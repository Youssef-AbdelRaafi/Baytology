using FluentValidation;

namespace Baytology.Application.Features.Conversations.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.Content)
            .Must((cmd, content) => !string.IsNullOrWhiteSpace(content) || !string.IsNullOrWhiteSpace(cmd.AttachmentUrl))
            .WithMessage("A message must include content or an attachment.");
        RuleFor(x => x.Content).MaximumLength(5000);
        RuleFor(x => x.AttachmentUrl).MaximumLength(1000);
    }
}
