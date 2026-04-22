using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Bookings.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery(
    string UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PaginatedList<BookingListItemDto>>>;
