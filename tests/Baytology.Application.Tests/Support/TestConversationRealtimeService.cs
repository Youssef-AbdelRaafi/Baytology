using Baytology.Application.Common.Interfaces;

namespace Baytology.Application.Tests.Support;

internal sealed class TestConversationRealtimeService : IConversationRealtimeService
{
    public List<ConversationRealtimeMessage> BroadcastMessages { get; } = [];

    public Task BroadcastMessageAsync(ConversationRealtimeMessage message, CancellationToken ct = default)
    {
        BroadcastMessages.Add(message);
        return Task.CompletedTask;
    }
}
