using Baytology.Application.Features.Conversations.Commands.SendMessage;
using Baytology.Application.Tests.Support;
using Baytology.Domain.Conversations;

namespace Baytology.Application.Tests.Conversations;

public sealed class SendMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_sends_message_and_updates_last_message_time()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SendMessageCommandHandler(context);

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var initialLastMessageAt = conversation.LastMessageAt;
        await Task.Delay(10);

        var command = new SendMessageCommand(conversation.Id, "buyer-1", "Is this available?", null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var message = Assert.Single(context.Messages);
        Assert.Equal("buyer-1", message.SenderId);
        Assert.Equal("Is this available?", message.Content);
        Assert.False(message.IsRead);
        Assert.True(conversation.LastMessageAt >= initialLastMessageAt);
    }

    [Fact]
    public async Task Handle_returns_not_found_for_nonexistent_conversation()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SendMessageCommandHandler(context);

        var command = new SendMessageCommand(Guid.NewGuid(), "buyer-1", "Hello", null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Conversation_Not_Found", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_rejects_message_from_non_participant()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new SendMessageCommandHandler(context);

        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var command = new SendMessageCommand(conversation.Id, "stranger-1", "Hi", null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Conversation_Unauthorized", result.TopError.Code);
        Assert.Empty(context.Messages);
    }
}
