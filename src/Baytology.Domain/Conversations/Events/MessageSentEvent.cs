using Baytology.Domain.Common;

namespace Baytology.Domain.Conversations.Events;

public sealed class MessageSentEvent(Guid messageId, Guid conversationId, string senderId) : DomainEvent
{
    public Guid MessageId { get; } = messageId;
    public Guid ConversationId { get; } = conversationId;
    public string SenderId { get; } = senderId;
}
