using Baytology.Domain.Common.Enums;

using FluentValidation;

namespace Baytology.Application.Features.AISearch.Commands.CreateSearchRequest;

public class CreateSearchRequestCommandValidator : AbstractValidator<CreateSearchRequestCommand>
{
    public CreateSearchRequestCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RawQuery).MaximumLength(2000);
        RuleFor(x => x.AudioFileUrl).MaximumLength(1000);
        RuleFor(x => x.ImageFileUrl).MaximumLength(1000);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.District).MaximumLength(100);
        RuleFor(x => x.PropertyType).MaximumLength(30);
        RuleFor(x => x.ListingType).MaximumLength(20);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.MinArea).GreaterThanOrEqualTo(0).When(x => x.MinArea.HasValue);
        RuleFor(x => x.MaxArea).GreaterThanOrEqualTo(0).When(x => x.MaxArea.HasValue);
        RuleFor(x => x.MinBedrooms).GreaterThanOrEqualTo(0).When(x => x.MinBedrooms.HasValue);
        RuleFor(x => x.MaxBedrooms).GreaterThanOrEqualTo(0).When(x => x.MaxBedrooms.HasValue);
        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice.Value <= x.MaxPrice.Value)
            .WithMessage("Minimum price cannot exceed maximum price.");
        RuleFor(x => x)
            .Must(x => !x.MinArea.HasValue || !x.MaxArea.HasValue || x.MinArea.Value <= x.MaxArea.Value)
            .WithMessage("Minimum area cannot exceed maximum area.");
        RuleFor(x => x)
            .Must(x => !x.MinBedrooms.HasValue || !x.MaxBedrooms.HasValue || x.MinBedrooms.Value <= x.MaxBedrooms.Value)
            .WithMessage("Minimum bedrooms cannot exceed maximum bedrooms.");
        RuleFor(x => x.RawQuery)
            .NotEmpty()
            .When(x => x.InputType == SearchInputType.Text)
            .WithMessage("RawQuery is required for text search.");
        RuleFor(x => x.AudioFileUrl)
            .NotEmpty()
            .When(x => x.InputType == SearchInputType.Voice)
            .WithMessage("AudioFileUrl is required for voice search.");
        RuleFor(x => x.ImageFileUrl)
            .NotEmpty()
            .When(x => x.InputType == SearchInputType.Image)
            .WithMessage("ImageFileUrl is required for image search.");
    }
}
