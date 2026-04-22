using Baytology.Domain.Common.Enums;

using FluentValidation;

namespace Baytology.Application.Features.Properties.Queries.GetProperties;

public sealed class GetPropertiesQueryValidator : AbstractValidator<GetPropertiesQuery>
{
    public GetPropertiesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice cannot be greater than MaxPrice.");

        RuleFor(x => x.MinArea)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinArea.HasValue);

        RuleFor(x => x.MaxArea)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxArea.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinArea.HasValue || !x.MaxArea.HasValue || x.MinArea <= x.MaxArea)
            .WithMessage("MinArea cannot be greater than MaxArea.");

        RuleFor(x => x.MinBedrooms)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinBedrooms.HasValue);

        RuleFor(x => x.MaxBedrooms)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxBedrooms.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinBedrooms.HasValue || !x.MaxBedrooms.HasValue || x.MinBedrooms <= x.MaxBedrooms)
            .WithMessage("MinBedrooms cannot be greater than MaxBedrooms.");

        RuleFor(x => x.PropertyType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<PropertyType>(value, true, out _))
            .WithMessage("PropertyType is invalid.");

        RuleFor(x => x.ListingType)
            .Must(value => string.IsNullOrWhiteSpace(value) || Enum.TryParse<ListingType>(value, true, out _))
            .WithMessage("ListingType is invalid.");
    }
}
