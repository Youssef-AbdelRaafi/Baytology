using Baytology.Domain.AuditLogs;

namespace Baytology.Domain.Tests.AuditLogs;

public sealed class AuditLogTests
{
    [Fact]
    public void Create_returns_validation_error_when_action_is_missing()
    {
        var result = AuditLog.Create(
            "user-1",
            null,
            "Property",
            "property-1",
            null,
            null,
            null);

        Assert.True(result.IsError);
        Assert.Equal(AuditLogErrors.ActionRequired.Code, result.TopError.Code);
    }
}
