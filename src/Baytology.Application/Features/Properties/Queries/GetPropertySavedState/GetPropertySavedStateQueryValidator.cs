using FluentValidation;

namespace Baytology.Application.Features.Properties.Queries.GetPropertySavedState;

public class GetPropertySavedStateQueryValidator : AbstractValidator<GetPropertySavedStateQuery>
{
    public GetPropertySavedStateQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PropertyId).NotEmpty();
    }
}
