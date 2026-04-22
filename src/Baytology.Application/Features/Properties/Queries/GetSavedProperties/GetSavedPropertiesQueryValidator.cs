using FluentValidation;

namespace Baytology.Application.Features.Properties.Queries.GetSavedProperties;

public sealed class GetSavedPropertiesQueryValidator : AbstractValidator<GetSavedPropertiesQuery>
{
    public GetSavedPropertiesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
