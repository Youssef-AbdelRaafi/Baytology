using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Properties;

public static class PropertyErrors
{
    public static readonly Error AgentRequired =
        Error.Validation("Property_Agent_Required", "Agent user ID is required.");

    public static readonly Error TitleRequired =
        Error.Validation("Property_Title_Required", "Property title is required.");

    public static readonly Error PriceInvalid =
        Error.Validation("Property_Price_Invalid", "Property price must be greater than zero.");

    public static readonly Error AreaInvalid =
        Error.Validation("Property_Area_Invalid", "Property area must be greater than zero.");

    public static readonly Error BedroomsInvalid =
        Error.Validation("Property_Bedrooms_Invalid", "Bedrooms cannot be negative.");

    public static readonly Error BathroomsInvalid =
        Error.Validation("Property_Bathrooms_Invalid", "Bathrooms cannot be negative.");

    public static readonly Error FloorInvalid =
        Error.Validation("Property_Floor_Invalid", "Floor cannot be negative.");

    public static readonly Error TotalFloorsInvalid =
        Error.Validation("Property_TotalFloors_Invalid", "Total floors must be greater than zero.");

    public static readonly Error FloorRangeInvalid =
        Error.Validation("Property_Floor_Range_Invalid", "Floor cannot exceed total floors.");

    public static readonly Error PropertyIdRequired =
        Error.Validation("Property_Id_Required", "Property id is required.");

    public static readonly Error ImageUrlRequired =
        Error.Validation("Property_ImageUrl_Required", "Image url is required.");

    public static readonly Error ImageUrlTooLong =
        Error.Validation("Property_ImageUrl_TooLong", "Image url cannot exceed 1000 characters.");

    public static readonly Error ImageSortOrderInvalid =
        Error.Validation("Property_ImageSortOrder_Invalid", "Image sort order cannot be negative.");

    public static readonly Error SavedPropertyUserRequired =
        Error.Validation("SavedProperty_User_Required", "User id is required.");

    public static readonly Error IpAddressTooLong =
        Error.Validation("PropertyView_IpAddress_TooLong", "IP address cannot exceed 50 characters.");

    public static readonly Error NotFound =
        Error.NotFound("Property_Not_Found", "Property not found.");

    public static readonly Error AlreadySaved =
        Error.Conflict("Property_Already_Saved", "Property is already saved.");

    public static readonly Error NotSaved =
        Error.NotFound("Property_Not_Saved", "Property is not in your saved list.");
}
