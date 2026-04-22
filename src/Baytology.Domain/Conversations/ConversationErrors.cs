using Baytology.Domain.Common.Results;

namespace Baytology.Domain.Conversations;

public static class ConversationErrors
{
    public static readonly Error PropertyRequired =
        Error.Validation("Conversation_Property_Required", "Property is required.");

    public static readonly Error BuyerRequired =
        Error.Validation("Conversation_Buyer_Required", "Buyer user is required.");

    public static readonly Error AgentRequired =
        Error.Validation("Conversation_Agent_Required", "Agent user is required.");

    public static readonly Error ParticipantsMustDiffer =
        Error.Validation("Conversation_Participants_Must_Differ", "Buyer and agent must be different users.");

    public static readonly Error ConversationIdRequired =
        Error.Validation("Conversation_Id_Required", "Conversation is required.");

    public static readonly Error NotFound =
        Error.NotFound("Conversation_Not_Found", "Conversation not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("Conversation_Already_Exists", "A conversation already exists for this property between these users.");

    public static readonly Error Unauthorized =
        Error.Unauthorized("Conversation_Unauthorized", "You are not a participant of this conversation.");

    public static readonly Error SenderRequired =
        Error.Validation("Conversation_Sender_Required", "Sender is required.");

    public static readonly Error MessageContentRequired =
        Error.Validation("Conversation_Message_Content_Required", "A message must include content or an attachment.");

    public static readonly Error MessageTooLong =
        Error.Validation("Conversation_Message_Too_Long", "Message content cannot exceed 5000 characters.");

    public static readonly Error AttachmentUrlTooLong =
        Error.Validation("Conversation_Attachment_Too_Long", "Attachment URL cannot exceed 1000 characters.");
}
