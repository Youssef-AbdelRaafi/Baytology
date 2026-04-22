using FluentValidation;

namespace Baytology.Application.Features.Conversations.Commands.CreateConversation;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.BuyerUserId).NotEmpty();
    }
}
