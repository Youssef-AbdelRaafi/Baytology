namespace Baytology.Contracts.Responses.Conversations;

public sealed record MessageResponse(
    Guid Id,
    Guid ConversationId,
    string SenderId,
    string Content,
    string? AttachmentUrl,
    bool IsRead,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt);
