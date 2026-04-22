namespace Baytology.Application.Features.Properties.Dtos;

public record PropertyAmenityDto(
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
