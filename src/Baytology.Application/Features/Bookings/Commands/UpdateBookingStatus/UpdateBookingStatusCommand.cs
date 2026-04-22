using Baytology.Application.Common.Caching;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Bookings;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Notifications;
using Baytology.Domain.Payments;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Bookings.Commands.UpdateBookingStatus;

public sealed record UpdateBookingStatusCommand(
    Guid BookingId,
    string ActorUserId,
    BookingStatus NewStatus) : IRequest<Result<bool>>, ICacheInvalidationRequest
{
    public IEnumerable<string> CacheTagsToInvalidate =>
    [
        ApplicationCacheTags.Properties,
        ApplicationCacheTags.SavedProperties
    ];
}
