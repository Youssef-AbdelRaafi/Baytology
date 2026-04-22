using FluentValidation;

namespace Baytology.Application.Features.AISearch.Queries.GetSearchRequest;

public sealed class GetSearchRequestQueryValidator : AbstractValidator<GetSearchRequestQuery>
{
    public GetSearchRequestQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
