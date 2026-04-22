using FluentValidation;

namespace Baytology.Application.Features.Bookings.Commands.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .Must(startDate => startDate.Date >= DateTimeOffset.UtcNow.Date)
            .WithMessage("StartDate cannot be in the past.");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CommissionRate).GreaterThanOrEqualTo(0).LessThan(1);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.PayerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.PayerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PayerPhone).NotEmpty().MaximumLength(50);
    }
}
