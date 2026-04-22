namespace Baytology.Application.Features.Properties.Dtos;

public record AgentReviewDto(
    Guid Id,
    string ReviewerUserId,
    Guid? PropertyId,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedOnUtc);
