using FluentValidation;

namespace Baytology.Application.Features.UserProfiles.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
