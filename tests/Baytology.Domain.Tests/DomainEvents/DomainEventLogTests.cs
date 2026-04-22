using Baytology.Domain.DomainEvents;

namespace Baytology.Domain.Tests.DomainEvents;

public sealed class DomainEventLogTests
{
    [Fact]
    public void Create_returns_validation_error_when_event_type_is_missing()
    {
        var result = DomainEventLog.Create(
            null,
            "aggregate-1",
            "Property",
            "{}");

        Assert.True(result.IsError);
        Assert.Equal(DomainEventLogErrors.EventTypeRequired.Code, result.TopError.Code);
    }
}
