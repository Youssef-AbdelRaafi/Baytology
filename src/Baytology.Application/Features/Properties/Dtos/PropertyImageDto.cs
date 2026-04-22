namespace Baytology.Application.Features.Properties.Dtos;

public record PropertyImageDto(
    Guid Id,
    string Url,
    bool IsPrimary,
    int SortOrder);
