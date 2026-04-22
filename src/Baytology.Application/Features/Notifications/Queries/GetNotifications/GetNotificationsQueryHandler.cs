using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var query = context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedOnUtc)
            .Select(n => new NotificationDto(
                n.Id, n.Type.ToString(), n.Title, n.Body,
                n.ReferenceId, n.ReferenceType.HasValue ? n.ReferenceType.Value.ToString() : null,
                n.IsRead, n.CreatedOnUtc))
            .ToListAsync(ct);

        return notifications;
    }
}
