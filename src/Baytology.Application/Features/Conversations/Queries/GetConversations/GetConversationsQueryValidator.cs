using FluentValidation;

namespace Baytology.Application.Features.Conversations.Queries.GetConversations;

public sealed class GetConversationsQueryValidator : AbstractValidator<GetConversationsQuery>
{
    public GetConversationsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
