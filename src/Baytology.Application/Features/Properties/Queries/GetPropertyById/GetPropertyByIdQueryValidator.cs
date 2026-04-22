using FluentValidation;

namespace Baytology.Application.Features.Properties.Queries.GetPropertyById;

public sealed class GetPropertyByIdQueryValidator : AbstractValidator<GetPropertyByIdQuery>
{
    public GetPropertyByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
