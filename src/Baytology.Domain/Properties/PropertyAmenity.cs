using Baytology.Domain.Common;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public sealed class PropertyAmenity : Entity
{
    public Guid PropertyId { get; private set; }
    public bool HasParking { get; private set; }
    public bool HasPool { get; private set; }
    public bool HasGym { get; private set; }
    public bool HasElevator { get; private set; }
    public bool HasSecurity { get; private set; }
    public bool HasBalcony { get; private set; }
    public bool HasGarden { get; private set; }
    public bool HasCentralAC { get; private set; }
    public FurnishingStatus FurnishingStatus { get; private set; }
    public ViewType? ViewType { get; private set; }

    private PropertyAmenity() { }

    private PropertyAmenity(Guid propertyId) : base(Guid.NewGuid())
    {
        PropertyId = propertyId;
        FurnishingStatus = FurnishingStatus.Unfurnished;
    }

    public static Result<PropertyAmenity> Create(Guid propertyId)
    {
        if (propertyId == Guid.Empty)
            return PropertyErrors.PropertyIdRequired;

        return new PropertyAmenity(propertyId);
    }

    public void Update(
        bool hasParking, bool hasPool, bool hasGym, bool hasElevator,
        bool hasSecurity, bool hasBalcony, bool hasGarden, bool hasCentralAC,
        FurnishingStatus furnishingStatus, ViewType? viewType)
    {
        HasParking = hasParking;
        HasPool = hasPool;
        HasGym = hasGym;
        HasElevator = hasElevator;
        HasSecurity = hasSecurity;
        HasBalcony = hasBalcony;
        HasGarden = hasGarden;
        HasCentralAC = hasCentralAC;
        FurnishingStatus = furnishingStatus;
        ViewType = viewType;
    }
}
