using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Notifications.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(IAppDbContext context)
    : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        // Ensure notification belongs to the authenticated user.
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, ct);

        if (notification is null)
            return ApplicationErrors.Notification.NotFound;

        if (!notification.MarkAsRead())
            return true;

        await context.SaveChangesAsync(ct);

        return true;
    }
}
