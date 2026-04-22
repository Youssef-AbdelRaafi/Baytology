using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Conversations.Commands.SendMessage;

using MediatR;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Infrastructure.RealTime;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}

[Authorize]
public class ChatHub(ISender sender, IAppDbContext context) : Hub
{
    public static string GetConversationGroupName(Guid conversationId)
        => $"conversation:{conversationId:N}";

    public async Task JoinConversation(string conversationId)
    {
        var conversationGuid = ParseConversationId(conversationId);
        var userId = GetCurrentUserId();

        await EnsureParticipantAsync(conversationGuid, userId, Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationGuid));
    }

    public async Task LeaveConversation(string conversationId)
    {
        var conversationGuid = ParseConversationId(conversationId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationGuid));
    }

    public async Task<Guid> SendMessage(string conversationId, string content, string? attachmentUrl = null)
    {
        var conversationGuid = ParseConversationId(conversationId);
        var userId = GetCurrentUserId();

        var result = await sender.Send(
            new SendMessageCommand(conversationGuid, userId, content, attachmentUrl),
            Context.ConnectionAborted);

        if (result.IsError)
            throw new HubException(result.TopError.Description);

        return result.Value;
    }

    private async Task EnsureParticipantAsync(Guid conversationId, string userId, CancellationToken ct)
    {
        var hasAccess = await context.Conversations
            .AsNoTracking()
            .AnyAsync(
                conversation => conversation.Id == conversationId &&
                (conversation.BuyerUserId == userId || conversation.AgentUserId == userId),
                ct);

        if (!hasAccess)
            throw new HubException("You are not allowed to access this conversation.");
    }

    private string GetCurrentUserId()
    {
        return Context.UserIdentifier
            ?? throw new HubException("Authenticated user identifier is missing.");
    }

    private static Guid ParseConversationId(string conversationId)
    {
        if (Guid.TryParse(conversationId, out var parsed))
            return parsed;

        throw new HubException("Invalid conversation id.");
    }
}
