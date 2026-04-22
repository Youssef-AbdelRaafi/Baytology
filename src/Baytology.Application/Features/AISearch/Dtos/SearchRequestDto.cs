namespace Baytology.Application.Features.AISearch.Dtos;

public record SearchRequestDto(
    Guid Id,
    string UserId,
    string InputType,
    string SearchEngine,
    string Status,
    int ResultCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    List<SearchResultDto> Results);
