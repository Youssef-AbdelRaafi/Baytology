using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.CreateAgentReview;

public class CreateAgentReviewCommandValidator : AbstractValidator<CreateAgentReviewCommand>
{
    public CreateAgentReviewCommandValidator()
    {
        RuleFor(x => x.AgentUserId).NotEmpty();
        RuleFor(x => x.ReviewerUserId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000);
    }
}
