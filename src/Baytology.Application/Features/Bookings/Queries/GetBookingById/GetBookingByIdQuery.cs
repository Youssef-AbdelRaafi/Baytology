using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Bookings.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery(Guid Id, string UserId) : IRequest<Result<BookingDto>>;
