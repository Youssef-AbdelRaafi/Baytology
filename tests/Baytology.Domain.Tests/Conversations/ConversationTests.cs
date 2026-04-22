using Baytology.Domain.Conversations;

namespace Baytology.Domain.Tests.Conversations;

public sealed class ConversationTests
{
    [Fact]
    public void Create_rejects_same_buyer_and_agent()
    {
        var result = Conversation.Create(Guid.NewGuid(), "user-1", "user-1");

        Assert.True(result.IsError);
        Assert.Equal("Conversation_Participants_Must_Differ", result.TopError.Code);
    }

    [Fact]
    public void Send_message_updates_last_message_time_and_tracks_message()
    {
        var conversation = Conversation.Create(Guid.NewGuid(), "buyer-1", "agent-1").Value;
        var beforeSend = conversation.LastMessageAt;

        Thread.Sleep(5);

        var messageResult = conversation.SendMessage("buyer-1", "Hello there", null);

        Assert.True(messageResult.IsSuccess);
        Assert.Single(conversation.Messages);
        Assert.True(conversation.LastMessageAt >= beforeSend);
    }
}
