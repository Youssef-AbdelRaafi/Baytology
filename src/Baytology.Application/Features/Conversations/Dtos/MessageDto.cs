namespace Baytology.Application.Features.Conversations.Dtos;

public record MessageDto(
    Guid Id,
    Guid ConversationId,
    string SenderId,
    string Content,
    string? AttachmentUrl,
    bool IsRead,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt);
