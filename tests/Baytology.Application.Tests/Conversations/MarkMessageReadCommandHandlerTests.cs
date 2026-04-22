using Baytology.Application.Features.Conversations.Commands.MarkMessageRead;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Conversations;

namespace Baytology.Application.Tests.Conversations;

public sealed class MarkMessageReadCommandHandlerTests
{
    [Fact]
    public async Task Recipient_can_mark_message_as_read_once()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new MarkMessageReadCommandHandler(context);

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        var message = conversation.SendMessage("buyer-1", "Hello").Value;

        context.Conversations.Add(conversation);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var firstResult = await handler.Handle(new MarkMessageReadCommand(message.Id, "agent-1"), CancellationToken.None);
        var secondResult = await handler.Handle(new MarkMessageReadCommand(message.Id, "agent-1"), CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.True(message.IsRead);
        Assert.NotNull(message.ReadAt);
    }

    [Fact]
    public async Task Sender_cannot_mark_own_message_as_read()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new MarkMessageReadCommandHandler(context);

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        var message = conversation.SendMessage("buyer-1", "Hello").Value;

        context.Conversations.Add(conversation);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new MarkMessageReadCommand(message.Id, "buyer-1"), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Message.ReadNotAllowed", result.TopError.Code);
        Assert.False(message.IsRead);
        Assert.Null(message.ReadAt);
    }
}
