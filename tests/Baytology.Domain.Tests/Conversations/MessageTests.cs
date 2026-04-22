using Baytology.Domain.Conversations;
using Baytology.Domain.Conversations.Events;

namespace Baytology.Domain.Tests.Conversations;

public sealed class MessageTests
{
    [Fact]
    public void Create_raises_message_sent_event_and_mark_as_read_sets_read_metadata()
    {
        var message = Message.Create(Guid.NewGuid(), "buyer-1", "Is this still available?", "https://files.test/doc.pdf").Value;

        Assert.False(message.IsRead);
        Assert.Equal("https://files.test/doc.pdf", message.AttachmentUrl);
        Assert.Single(message.DomainEvents);
        Assert.IsType<MessageSentEvent>(message.DomainEvents.Single());

        Assert.True(message.MarkAsRead());
        Assert.False(message.MarkAsRead());

        Assert.True(message.IsRead);
        Assert.NotNull(message.ReadAt);
    }
}
