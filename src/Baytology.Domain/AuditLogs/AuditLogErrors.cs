using Baytology.Domain.Common.Results;

namespace Baytology.Domain.AuditLogs;

public static class AuditLogErrors
{
    public static readonly Error ActionRequired =
        Error.Validation("AuditLog_Action_Required", "Audit action is required.");

    public static readonly Error ActionTooLong =
        Error.Validation("AuditLog_Action_TooLong", "Audit action cannot exceed 20 characters.");

    public static readonly Error EntityNameRequired =
        Error.Validation("AuditLog_EntityName_Required", "Audit entity name is required.");

    public static readonly Error EntityNameTooLong =
        Error.Validation("AuditLog_EntityName_TooLong", "Audit entity name cannot exceed 100 characters.");

    public static readonly Error EntityIdRequired =
        Error.Validation("AuditLog_EntityId_Required", "Audit entity id is required.");

    public static readonly Error EntityIdTooLong =
        Error.Validation("AuditLog_EntityId_TooLong", "Audit entity id cannot exceed 200 characters.");

    public static readonly Error IpAddressTooLong =
        Error.Validation("AuditLog_IpAddress_TooLong", "Audit IP address cannot exceed 50 characters.");
}
