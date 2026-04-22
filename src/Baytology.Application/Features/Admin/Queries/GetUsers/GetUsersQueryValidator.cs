using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetUsers;

public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
    }
}
