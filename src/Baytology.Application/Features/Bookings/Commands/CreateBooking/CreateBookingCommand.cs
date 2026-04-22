using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Constants;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid PropertyId,
    string UserId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Amount,
    decimal CommissionRate,
    string Currency,
    string PayerEmail,
    string PayerName,
    string PayerPhone) : IRequest<Result<CreateBookingResponse>>;

public sealed record CreateBookingResponse(
    Guid BookingId,
    Guid PaymentId,
    string? RedirectUrl);
