using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetRecommendationRequests;

public sealed class GetRecommendationRequestsQueryValidator : AbstractValidator<GetRecommendationRequestsQuery>
{
    public GetRecommendationRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
