using FluentValidation;

namespace Baytology.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
        RuleFor(x => x.ExpiredAccessToken).NotEmpty();
    }
}
