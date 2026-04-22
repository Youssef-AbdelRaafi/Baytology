using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Notifications;
using Baytology.Infrastructure.Data;

namespace Baytology.Api.Tests.Infrastructure;

internal sealed class DatabaseNotificationService(AppDbContext context) : INotificationService
{
    public async Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(ct);
    }
}
