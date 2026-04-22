namespace Baytology.Contracts.Responses.Properties;

public sealed record PropertyAmenityResponse(
    bool HasParking,
    bool HasPool,
    bool HasGym,
    bool HasElevator,
    bool HasSecurity,
    bool HasBalcony,
    bool HasGarden,
    bool HasCentralAC,
    string FurnishingStatus,
    string? ViewType);
