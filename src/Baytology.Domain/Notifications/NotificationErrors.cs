using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Notifications;

public static class NotificationErrors
{
    public static readonly Error UserRequired =
        Error.Validation("Notification_User_Required", "Notification user is required.");

    public static readonly Error TitleRequired =
        Error.Validation("Notification_Title_Required", "Notification title is required.");

    public static readonly Error TitleTooLong =
        Error.Validation("Notification_Title_Too_Long", "Notification title cannot exceed 300 characters.");

    public static readonly Error BodyRequired =
        Error.Validation("Notification_Body_Required", "Notification body is required.");

    public static readonly Error BodyTooLong =
        Error.Validation("Notification_Body_Too_Long", "Notification body cannot exceed 2000 characters.");

    public static readonly Error ReferencePairRequired =
        Error.Validation("Notification_Reference_Invalid", "Reference id and reference type must either both be provided or both be omitted.");

    public static readonly Error ReferenceIdTooLong =
        Error.Validation("Notification_ReferenceId_Too_Long", "Reference id cannot exceed 100 characters.");
}
