using FluentValidation;

namespace Baytology.Application.Features.Recommendations.Commands.CreateRecommendationRequest;

public class CreateRecommendationRequestCommandValidator : AbstractValidator<CreateRecommendationRequestCommand>
{
    public CreateRecommendationRequestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SourceEntityType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SourceEntityId).MaximumLength(200);
        RuleFor(x => x.TopN).InclusiveBetween(1, 50);
    }
}
