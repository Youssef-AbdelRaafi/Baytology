using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.CreateProperty;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.AgentUserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Area).GreaterThan(0);
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(0).When(x => x.Floor.HasValue);
        RuleFor(x => x.TotalFloors).GreaterThan(0).When(x => x.TotalFloors.HasValue);
        RuleFor(x => x)
            .Must(x => !x.Floor.HasValue || !x.TotalFloors.HasValue || x.Floor.Value <= x.TotalFloors.Value)
            .WithMessage("Floor cannot exceed total floors.");
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(1000);
    }
}
