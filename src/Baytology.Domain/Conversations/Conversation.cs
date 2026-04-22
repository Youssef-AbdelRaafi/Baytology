using Baytology.Domain.Common;
using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Conversations;

public sealed class Conversation : Entity
{
    public Guid PropertyId { get; private set; }
    public string BuyerUserId { get; private set; } = null!;
    public string AgentUserId { get; private set; } = null!;
    public DateTimeOffset CreatedOnUtc { get; private set; }
    public DateTimeOffset LastMessageAt { get; private set; }

    private readonly List<Message> _messages = [];
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    private Conversation(Guid propertyId, string buyerUserId, string agentUserId)
        : base(Guid.NewGuid())
    {
        PropertyId = propertyId;
        BuyerUserId = buyerUserId;
        AgentUserId = agentUserId;
        CreatedOnUtc = DateTimeOffset.UtcNow;
        LastMessageAt = DateTimeOffset.UtcNow;
    }

    public static Result<Conversation> Create(Guid propertyId, string buyerUserId, string agentUserId)
    {
        if (propertyId == Guid.Empty)
            return ConversationErrors.PropertyRequired;

        if (string.IsNullOrWhiteSpace(buyerUserId))
            return ConversationErrors.BuyerRequired;

        if (string.IsNullOrWhiteSpace(agentUserId))
            return ConversationErrors.AgentRequired;

        if (string.Equals(buyerUserId, agentUserId, StringComparison.Ordinal))
            return ConversationErrors.ParticipantsMustDiffer;

        return new Conversation(propertyId, buyerUserId.Trim(), agentUserId.Trim());
    }

    public Result<Message> SendMessage(string senderId, string? content, string? attachmentUrl = null)
    {
        if (string.IsNullOrWhiteSpace(senderId))
            return ConversationErrors.SenderRequired;

        var normalizedSenderId = senderId.Trim();
        if (normalizedSenderId != BuyerUserId && normalizedSenderId != AgentUserId)
            return ConversationErrors.Unauthorized;

        var messageResult = Message.Create(Id, normalizedSenderId, content, attachmentUrl);
        if (messageResult.IsError)
            return messageResult.Errors;

        _messages.Add(messageResult.Value);
        LastMessageAt = DateTimeOffset.UtcNow;
        return messageResult.Value;
    }
}
