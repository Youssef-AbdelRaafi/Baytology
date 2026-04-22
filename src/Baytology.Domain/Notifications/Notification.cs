using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Notifications;

public sealed class Notification : Entity
{
    public string UserId { get; private set; } = null!;
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public string? ReferenceId { get; private set; }
    public ReferenceType? ReferenceType { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    private Notification(
        string userId,
        NotificationType type,
        string title,
        string body,
        string? referenceId = null,
        ReferenceType? referenceType = null) : base(Guid.NewGuid())
    {
        UserId = userId;
        Type = type;
        Title = title;
        Body = body;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        IsRead = false;
        CreatedOnUtc = DateTimeOffset.UtcNow;
    }

    public static Result<Notification> Create(
        string userId,
        NotificationType type,
        string title,
        string body,
        string? referenceId = null,
        ReferenceType? referenceType = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return NotificationErrors.UserRequired;

        var normalizedTitle = title?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedTitle))
            return NotificationErrors.TitleRequired;

        if (normalizedTitle.Length > 300)
            return NotificationErrors.TitleTooLong;

        var normalizedBody = body?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedBody))
            return NotificationErrors.BodyRequired;

        if (normalizedBody.Length > 2000)
            return NotificationErrors.BodyTooLong;

        var normalizedReferenceId = string.IsNullOrWhiteSpace(referenceId) ? null : referenceId.Trim();
        if ((normalizedReferenceId is null) != !referenceType.HasValue)
            return NotificationErrors.ReferencePairRequired;

        if (normalizedReferenceId is not null && normalizedReferenceId.Length > 100)
            return NotificationErrors.ReferenceIdTooLong;

        return new Notification(userId.Trim(), type, normalizedTitle, normalizedBody, normalizedReferenceId, referenceType);
    }

    public bool MarkAsRead()
    {
        if (IsRead)
            return false;

        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        return true;
    }
}
