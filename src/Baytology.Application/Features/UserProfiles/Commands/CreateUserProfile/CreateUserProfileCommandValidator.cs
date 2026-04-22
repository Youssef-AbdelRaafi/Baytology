using FluentValidation;

namespace Baytology.Application.Features.UserProfiles.Commands.CreateUserProfile;

public class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200).WithMessage("Display name is required.");
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
