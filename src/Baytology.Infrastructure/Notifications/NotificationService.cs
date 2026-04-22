using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Notifications;
using Baytology.Infrastructure.Data;
using Baytology.Infrastructure.RealTime;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Baytology.Infrastructure.Notifications;

public class NotificationService(
    AppDbContext context,
    IHubContext<NotificationHub> hubContext,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        // Persist to DB
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(ct);

        // Push via SignalR
        try
        {
            await hubContext.Clients.User(notification.UserId)
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    Type = notification.Type.ToString(),
                    notification.Title,
                    notification.Body,
                    notification.ReferenceId,
                    ReferenceType = notification.ReferenceType.ToString(),
                    notification.IsRead,
                    notification.CreatedOnUtc
                }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification to user {UserId}", notification.UserId);
        }
    }
}
