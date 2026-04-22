namespace Baytology.Contracts.Requests.Conversations;

public sealed record SendMessageRequest(
    string Content,
    string? AttachmentUrl);
