using FluentValidation;

namespace Baytology.Application.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
