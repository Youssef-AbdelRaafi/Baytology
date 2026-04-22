using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetRefundRequests;

public sealed class GetRefundRequestsQueryValidator : AbstractValidator<GetRefundRequestsQuery>
{
    public GetRefundRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
