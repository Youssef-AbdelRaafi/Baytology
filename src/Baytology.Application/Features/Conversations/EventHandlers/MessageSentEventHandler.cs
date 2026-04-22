using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Conversations.Events;
using Baytology.Domain.Notifications;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.Conversations.EventHandlers;

public class MessageSentEventHandler(
    IAppDbContext context,
    INotificationService notificationService,
    IConversationRealtimeService realtimeService,
    ILogger<MessageSentEventHandler> logger)
    : INotificationHandler<MessageSentEvent>
{
    public async Task Handle(MessageSentEvent notification, CancellationToken ct)
    {
        var conversation = await context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == notification.ConversationId, ct);

        if (conversation is null)
            return;

        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == notification.MessageId, ct);

        // Determine recipient
        var recipientId = conversation.BuyerUserId == notification.SenderId
            ? conversation.AgentUserId
            : conversation.BuyerUserId;

        var notifResult = Notification.Create(
            recipientId,
            NotificationType.NewMessage,
            "New Message",
            "You have a new message.",
            notification.MessageId.ToString(),
            ReferenceType.Message);

        if (notifResult.IsError)
            return;

        try
        {
            await notificationService.SendAsync(notifResult.Value, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to persist or deliver notification for message {MessageId}. The message remains saved.",
                notification.MessageId);
        }

        if (message is null)
            return;

        try
        {
            await realtimeService.BroadcastMessageAsync(
                new ConversationRealtimeMessage(
                    message.Id,
                    message.ConversationId,
                    message.SenderId,
                    message.Content,
                    message.AttachmentUrl,
                    message.SentAt),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to broadcast realtime message {MessageId} for conversation {ConversationId}.",
                message.Id,
                message.ConversationId);
        }
    }
}
