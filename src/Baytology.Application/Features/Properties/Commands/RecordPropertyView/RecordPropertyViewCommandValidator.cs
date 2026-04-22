using FluentValidation;

namespace Baytology.Application.Features.Properties.Commands.RecordPropertyView;

public class RecordPropertyViewCommandValidator : AbstractValidator<RecordPropertyViewCommand>
{
    public RecordPropertyViewCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.IpAddress).MaximumLength(50);
    }
}
