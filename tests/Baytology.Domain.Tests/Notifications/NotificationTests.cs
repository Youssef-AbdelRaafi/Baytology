using Baytology.Domain.Common.Enums;
using Baytology.Domain.Notifications;

namespace Baytology.Domain.Tests.Notifications;

public sealed class NotificationTests
{
    [Fact]
    public void Create_requires_reference_id_and_type_together()
    {
        var result = Notification.Create(
            "user-1",
            NotificationType.PaymentUpdate,
            "Payment update",
            "Body",
            referenceId: "123",
            referenceType: null);

        Assert.True(result.IsError);
        Assert.Equal("Notification_Reference_Invalid", result.TopError.Code);
    }

    [Fact]
    public void Mark_as_read_is_idempotent()
    {
        var notification = Notification.Create(
            "user-1",
            NotificationType.NewMessage,
            "New message",
            "You have a new message.").Value;

        Assert.True(notification.MarkAsRead());
        Assert.False(notification.MarkAsRead());
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
    }
}
