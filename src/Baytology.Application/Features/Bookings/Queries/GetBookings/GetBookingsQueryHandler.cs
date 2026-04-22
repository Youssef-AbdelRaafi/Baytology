using Baytology.Application.Common.Interfaces;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Bookings.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Queries.GetBookings;

public class GetBookingsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetBookingsQuery, Result<PaginatedList<BookingListItemDto>>>
{
    public async Task<Result<PaginatedList<BookingListItemDto>>> Handle(GetBookingsQuery request, CancellationToken ct)
    {
        var query = context.Bookings.AsNoTracking()
            .Where(b => b.UserId == request.UserId || b.AgentUserId == request.UserId);

        var projected = query
            .OrderByDescending(b => b.CreatedOnUtc)
            .Select(b => new BookingListItemDto(
                b.Id,
                b.PropertyId,
                context.Properties.Where(p => p.Id == b.PropertyId).Select(p => p.Title).FirstOrDefault() ?? "Unknown",
                b.StartDate,
                b.EndDate,
                b.Status.ToString(),
                b.CreatedOnUtc));

        return await PaginatedList<BookingListItemDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
    }
}
