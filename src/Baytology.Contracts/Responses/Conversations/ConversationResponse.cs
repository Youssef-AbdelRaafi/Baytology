namespace Baytology.Contracts.Responses.Conversations;

public sealed record ConversationResponse(
    Guid Id,
    Guid PropertyId,
    string BuyerUserId,
    string AgentUserId,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset LastMessageAt,
    string? LastMessageContent);
