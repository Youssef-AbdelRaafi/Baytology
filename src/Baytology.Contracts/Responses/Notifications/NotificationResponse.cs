namespace Baytology.Contracts.Responses.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTimeOffset CreatedOnUtc);
