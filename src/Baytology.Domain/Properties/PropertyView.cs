using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Properties.Events;

namespace Baytology.Domain.Properties;

public sealed class PropertyView : Entity
{
    public Guid PropertyId { get; private set; }
    public string? UserId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTimeOffset ViewedAt { get; private set; }

    private PropertyView() { }

    private PropertyView(Guid propertyId, string? userId, string? ipAddress) : base(Guid.NewGuid())
    {
        PropertyId = propertyId;
        UserId = userId;
        IpAddress = ipAddress;
        ViewedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new PropertyViewedEvent(propertyId, userId));
    }

    public static Result<PropertyView> Create(Guid propertyId, string? userId, string? ipAddress)
    {
        if (propertyId == Guid.Empty)
            return PropertyErrors.PropertyIdRequired;

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        var normalizedIpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();

        if (normalizedIpAddress is not null && normalizedIpAddress.Length > 50)
            return PropertyErrors.IpAddressTooLong;

        return new PropertyView(propertyId, normalizedUserId, normalizedIpAddress);
    }
}
