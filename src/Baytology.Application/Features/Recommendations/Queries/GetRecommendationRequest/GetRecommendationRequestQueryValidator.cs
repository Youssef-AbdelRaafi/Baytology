using FluentValidation;

namespace Baytology.Application.Features.Recommendations.Queries.GetRecommendationRequest;

public sealed class GetRecommendationRequestQueryValidator : AbstractValidator<GetRecommendationRequestQuery>
{
    public GetRecommendationRequestQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
