using Baytology.Domain.Common.Enums;

using FluentValidation;

namespace Baytology.Application.Features.Bookings.Commands.UpdateBookingStatus;

public class UpdateBookingStatusCommandValidator : AbstractValidator<UpdateBookingStatusCommand>
{
    public UpdateBookingStatusCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .Must(status => status is BookingStatus.Confirmed or BookingStatus.Cancelled)
            .WithMessage("Booking status can only be updated to Confirmed or Cancelled from this endpoint.");
    }
}
