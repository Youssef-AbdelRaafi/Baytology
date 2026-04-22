using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Conversations.Events;

namespace Baytology.Domain.Conversations;

public sealed class Message : Entity
{
    public Guid ConversationId { get; private set; }
    public string SenderId { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string? AttachmentUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Message() { }

    private Message(Guid conversationId, string senderId, string content, string? attachmentUrl = null)
        : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        AttachmentUrl = attachmentUrl;
        IsRead = false;
        SentAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new MessageSentEvent(Id, conversationId, senderId));
    }

    public static Result<Message> Create(Guid conversationId, string senderId, string? content, string? attachmentUrl = null)
    {
        if (conversationId == Guid.Empty)
            return ConversationErrors.ConversationIdRequired;

        if (string.IsNullOrWhiteSpace(senderId))
            return ConversationErrors.SenderRequired;

        var normalizedContent = string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim();
        var normalizedAttachmentUrl = string.IsNullOrWhiteSpace(attachmentUrl) ? null : attachmentUrl.Trim();

        if (string.IsNullOrWhiteSpace(normalizedContent) && string.IsNullOrWhiteSpace(normalizedAttachmentUrl))
            return ConversationErrors.MessageContentRequired;

        if (normalizedContent.Length > 5000)
            return ConversationErrors.MessageTooLong;

        if (normalizedAttachmentUrl is not null && normalizedAttachmentUrl.Length > 1000)
            return ConversationErrors.AttachmentUrlTooLong;

        return new Message(conversationId, senderId.Trim(), normalizedContent, normalizedAttachmentUrl);
    }

    public bool MarkAsRead()
    {
        if (IsRead)
            return false;

        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
        return true;
    }
}
