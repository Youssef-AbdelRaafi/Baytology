using FluentValidation;

namespace Baytology.Application.Features.Recommendations.Commands.CompleteRecommendationRequest;

public sealed class CompleteRecommendationRequestCommandValidator : AbstractValidator<CompleteRecommendationRequestCommand>
{
    public CompleteRecommendationRequestCommandValidator()
    {
        RuleFor(x => x.RecommendationRequestId).NotEmpty();

        RuleFor(x => x.Results)
            .NotNull()
            .When(x => x.IsSuccessful);

        RuleForEach(x => x.Results).ChildRules(result =>
        {
            result.RuleFor(x => x.Rank).GreaterThan(0);
            result.RuleFor(x => x.SimilarityScore).GreaterThanOrEqualTo(0);
            result.RuleFor(x => x.SnapshotTitle).MaximumLength(500);
            result.RuleFor(x => x.ExternalReference).MaximumLength(500);
            result.RuleFor(x => x.SnapshotPrice).GreaterThanOrEqualTo(0).When(x => x.SnapshotPrice.HasValue);
            result.RuleFor(x => x)
                .Must(x => x.RecommendedPropertyId.HasValue || !string.IsNullOrWhiteSpace(x.ExternalReference))
                .WithMessage("Each recommendation result must include a property id or an external reference.");
        });
    }
}
