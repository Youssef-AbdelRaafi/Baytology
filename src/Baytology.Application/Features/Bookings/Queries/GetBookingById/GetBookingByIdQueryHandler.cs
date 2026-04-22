using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Bookings.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Queries.GetBookingById;

public class GetBookingByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken ct)
    {
        var booking = await context.Bookings.AsNoTracking()
            .Where(b => b.Id == request.Id)
            .Select(b => new BookingDto(
                b.Id,
                b.PropertyId,
                context.Properties.Where(p => p.Id == b.PropertyId).Select(p => p.Title).FirstOrDefault() ?? "Unknown",
                b.UserId,
                b.AgentUserId,
                b.StartDate,
                b.EndDate,
                b.Status.ToString(),
                b.PaymentId != null
                    ? context.Payments.Where(p => p.Id == b.PaymentId).Select(p => p.Amount).FirstOrDefault()
                    : 0m,
                b.PaymentId != null
                    ? context.Payments.Where(p => p.Id == b.PaymentId).Select(p => p.Currency).FirstOrDefault() ?? "EGP"
                    : "EGP",
                b.PaymentId != null
                    ? context.Payments
                        .Where(p => p.Id == b.PaymentId)
                        .Select(p => p.Amount > 0 ? p.Commission / p.Amount : 0m)
                        .FirstOrDefault()
                    : 0m,
                b.PaymentId,
                b.CreatedOnUtc))
            .FirstOrDefaultAsync(ct);

        if (booking is null)
            return ApplicationErrors.Booking.NotFound;

        if (booking.UserId != request.UserId && booking.AgentUserId != request.UserId)
            return ApplicationErrors.Booking.AccessDenied;

        return booking;
    }
}
