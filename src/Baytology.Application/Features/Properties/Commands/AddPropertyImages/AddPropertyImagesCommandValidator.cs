using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.AddPropertyImages;

public class AddPropertyImagesCommandValidator : AbstractValidator<AddPropertyImagesCommand>
{
    public AddPropertyImagesCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.AgentUserId).NotEmpty();
        RuleFor(x => x.ImageUrls).NotEmpty();
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(1000);
    }
}
