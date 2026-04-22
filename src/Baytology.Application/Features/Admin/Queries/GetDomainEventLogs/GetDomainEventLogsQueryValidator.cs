using FluentValidation;

namespace Baytology.Application.Features.Admin.Queries.GetDomainEventLogs;

public sealed class GetDomainEventLogsQueryValidator : AbstractValidator<GetDomainEventLogsQuery>
{
    public GetDomainEventLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
