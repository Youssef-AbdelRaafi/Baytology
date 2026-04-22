using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public sealed class PropertyImage : Entity
{
    public Guid PropertyId { get; private set; }
    public string Url { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private PropertyImage() { }

    private PropertyImage(Guid propertyId, string url, bool isPrimary, int sortOrder)
        : base(Guid.NewGuid())
    {
        PropertyId = propertyId;
        Url = url;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
        UploadedAt = DateTimeOffset.UtcNow;
    }

    public static Result<PropertyImage> Create(Guid propertyId, string? url, bool isPrimary, int sortOrder)
    {
        if (propertyId == Guid.Empty)
            return PropertyErrors.PropertyIdRequired;

        var normalizedUrl = url?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUrl))
            return PropertyErrors.ImageUrlRequired;

        if (normalizedUrl.Length > 1000)
            return PropertyErrors.ImageUrlTooLong;

        if (sortOrder < 0)
            return PropertyErrors.ImageSortOrderInvalid;

        return new PropertyImage(propertyId, normalizedUrl, isPrimary, sortOrder);
    }
}
