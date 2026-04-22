using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetSearchRequests;

public sealed class GetSearchRequestsQueryValidator : AbstractValidator<GetSearchRequestsQuery>
{
    public GetSearchRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
