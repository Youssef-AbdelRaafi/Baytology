using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AuditLogs;

public sealed class AuditLog : Entity
{
    public string? UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityName { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset OccurredOnUtc { get; private set; }

    private AuditLog() { }

    private AuditLog(
        string? userId,
        string action,
        string entityName,
        string entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress) : base(Guid.NewGuid())
    {
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        OccurredOnUtc = DateTimeOffset.UtcNow;
    }

    public static Result<AuditLog> Create(
        string? userId,
        string? action,
        string? entityName,
        string? entityId,
        string? oldValues,
        string? newValues,
        string? ipAddress)
    {
        var normalizedAction = action?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedAction))
            return AuditLogErrors.ActionRequired;

        if (normalizedAction.Length > 20)
            return AuditLogErrors.ActionTooLong;

        var normalizedEntityName = entityName?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEntityName))
            return AuditLogErrors.EntityNameRequired;

        if (normalizedEntityName.Length > 100)
            return AuditLogErrors.EntityNameTooLong;

        var normalizedEntityId = entityId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEntityId))
            return AuditLogErrors.EntityIdRequired;

        if (normalizedEntityId.Length > 200)
            return AuditLogErrors.EntityIdTooLong;

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        var normalizedIpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();

        if (normalizedIpAddress is not null && normalizedIpAddress.Length > 50)
            return AuditLogErrors.IpAddressTooLong;

        return new AuditLog(
            normalizedUserId,
            normalizedAction,
            normalizedEntityName,
            normalizedEntityId,
            oldValues,
            newValues,
            normalizedIpAddress);
    }
}
