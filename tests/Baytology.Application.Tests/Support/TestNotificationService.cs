using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Notifications;

namespace Baytology.Application.Tests.Support;

internal sealed class TestNotificationService : INotificationService
{
    public List<Notification> SentNotifications { get; } = [];

    public Task SendAsync(Notification notification, CancellationToken ct = default)
    {
        SentNotifications.Add(notification);
        return Task.CompletedTask;
    }
}
