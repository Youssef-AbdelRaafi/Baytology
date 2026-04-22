using Baytology.Application.Common.Interfaces;

using Microsoft.AspNetCore.SignalR;

namespace Baytology.Infrastructure.RealTime;

internal sealed class ConversationRealtimeService(IHubContext<ChatHub> hubContext) : IConversationRealtimeService
{
    public Task BroadcastMessageAsync(ConversationRealtimeMessage message, CancellationToken ct = default)
    {
        return hubContext.Clients
            .Group(ChatHub.GetConversationGroupName(message.ConversationId))
            .SendAsync("ReceiveMessage", new
            {
                Id = message.MessageId,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.AttachmentUrl,
                message.SentAt,
                IsRead = false
            }, ct);
    }
}
