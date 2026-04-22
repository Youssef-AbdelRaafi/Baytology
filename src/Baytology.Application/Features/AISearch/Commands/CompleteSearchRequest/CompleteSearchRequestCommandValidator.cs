using FluentValidation;

namespace Baytology.Application.Features.AISearch.Commands.CompleteSearchRequest;

public sealed class CompleteSearchRequestCommandValidator : AbstractValidator<CompleteSearchRequestCommand>
{
    public CompleteSearchRequestCommandValidator()
    {
        RuleFor(x => x.SearchRequestId).NotEmpty();

        RuleFor(x => x.Results)
            .NotNull()
            .When(x => x.IsSuccessful);

        RuleForEach(x => x.Results).ChildRules(result =>
        {
            result.RuleFor(x => x.PropertyId).NotEmpty();
            result.RuleFor(x => x.Rank).GreaterThan(0);
            result.RuleFor(x => x.RelevanceScore).GreaterThanOrEqualTo(0);
            result.RuleFor(x => x.ScoreSource).MaximumLength(50);
            result.RuleFor(x => x.SnapshotTitle).MaximumLength(500);
            result.RuleFor(x => x.SnapshotCity).MaximumLength(100);
            result.RuleFor(x => x.SnapshotStatus).MaximumLength(30);
            result.RuleFor(x => x.SnapshotPrice).GreaterThanOrEqualTo(0).When(x => x.SnapshotPrice.HasValue);
        });
    }
}
