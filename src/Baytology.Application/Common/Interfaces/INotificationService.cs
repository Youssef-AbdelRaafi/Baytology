using Baytology.Domain.Notifications;

namespace Baytology.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(Notification notification, CancellationToken ct = default);
}
