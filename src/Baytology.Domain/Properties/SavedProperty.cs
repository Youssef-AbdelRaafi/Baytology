using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public sealed class SavedProperty : Entity
{
    public string UserId { get; private set; } = null!;
    public Guid PropertyId { get; private set; }
    public DateTimeOffset SavedAt { get; private set; }

    private SavedProperty() { }

    private SavedProperty(string userId, Guid propertyId) : base(Guid.NewGuid())
    {
        UserId = userId;
        PropertyId = propertyId;
        SavedAt = DateTimeOffset.UtcNow;
    }

    public static Result<SavedProperty> Create(string? userId, Guid propertyId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return PropertyErrors.SavedPropertyUserRequired;

        if (propertyId == Guid.Empty)
            return PropertyErrors.PropertyIdRequired;

        return new SavedProperty(userId.Trim(), propertyId);
    }
}
