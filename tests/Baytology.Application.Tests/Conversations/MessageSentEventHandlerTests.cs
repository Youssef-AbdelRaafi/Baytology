using Baytology.Application.Features.Conversations.EventHandlers;
using Baytology.Application.Tests.Support;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Conversations;
using Baytology.Domain.Conversations.Events;

using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.Conversations;

public sealed class MessageSentEventHandlerTests
{
    [Fact]
    public async Task Message_sent_event_notifies_recipient_and_broadcasts_to_conversation_group()
    {
        using var context = TestDbContextFactory.Create();
        var notificationService = new TestNotificationService();
        var realtimeService = new TestConversationRealtimeService();

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        var message = conversation.SendMessage("buyer-1", "Hello agent", "https://files.test/proof.pdf").Value;

        context.Conversations.Add(conversation);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var handler = new MessageSentEventHandler(
            context,
            notificationService,
            realtimeService,
            NullLogger<MessageSentEventHandler>.Instance);

        await handler.Handle(new MessageSentEvent(message.Id, conversation.Id, message.SenderId), CancellationToken.None);

        Assert.Single(notificationService.SentNotifications);
        Assert.Equal("agent-1", notificationService.SentNotifications[0].UserId);

        Assert.Single(realtimeService.BroadcastMessages);
        Assert.Equal(message.Id, realtimeService.BroadcastMessages[0].MessageId);
        Assert.Equal(conversation.Id, realtimeService.BroadcastMessages[0].ConversationId);
        Assert.Equal("Hello agent", realtimeService.BroadcastMessages[0].Content);
        Assert.Equal("https://files.test/proof.pdf", realtimeService.BroadcastMessages[0].AttachmentUrl);
    }

    [Fact]
    public async Task Message_sent_event_remains_best_effort_when_notification_and_realtime_delivery_fail()
    {
        using var context = TestDbContextFactory.Create();

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        var message = conversation.SendMessage("buyer-1", "Hello again").Value;

        context.Conversations.Add(conversation);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var handler = new MessageSentEventHandler(
            context,
            new ThrowingNotificationService(),
            new ThrowingConversationRealtimeService(),
            NullLogger<MessageSentEventHandler>.Instance);

        await handler.Handle(new MessageSentEvent(message.Id, conversation.Id, message.SenderId), CancellationToken.None);

        var persistedMessage = await context.Messages.FindAsync(message.Id);
        Assert.NotNull(persistedMessage);
        Assert.False(persistedMessage!.IsRead);
    }

    private sealed class ThrowingNotificationService : INotificationService
    {
        public Task SendAsync(Baytology.Domain.Notifications.Notification notification, CancellationToken ct = default)
            => throw new InvalidOperationException("Notification persistence is unavailable.");
    }

    private sealed class ThrowingConversationRealtimeService : IConversationRealtimeService
    {
        public Task BroadcastMessageAsync(ConversationRealtimeMessage message, CancellationToken ct = default)
            => throw new InvalidOperationException("Realtime delivery is unavailable.");
    }
}
