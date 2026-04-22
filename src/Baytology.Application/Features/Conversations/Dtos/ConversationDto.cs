namespace Baytology.Application.Features.Conversations.Dtos;

public record ConversationDto(
    Guid Id,
    Guid PropertyId,
    string BuyerUserId,
    string AgentUserId,
    string? BuyerDisplayName,
    string? AgentDisplayName,
    string? PropertyTitle,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset LastMessageAt,
    string? LastMessageContent);
