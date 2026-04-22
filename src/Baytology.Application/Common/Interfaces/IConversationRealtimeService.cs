namespace Baytology.Application.Common.Interfaces;

public interface IConversationRealtimeService
{
    Task BroadcastMessageAsync(ConversationRealtimeMessage message, CancellationToken ct = default);
}

public sealed record ConversationRealtimeMessage(
    Guid MessageId,
    Guid ConversationId,
    string SenderId,
    string Content,
    string? AttachmentUrl,
    DateTimeOffset SentAt);
