using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Commands.MarkMessageRead;

public class MarkMessageReadCommandHandler(IAppDbContext context)
    : IRequestHandler<MarkMessageReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkMessageReadCommand request, CancellationToken ct)
    {
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, ct);

        if (message is null)
            return ApplicationErrors.Conversation.MessageNotFound;

        var conversation = await context.Conversations
            .FirstOrDefaultAsync(c => c.Id == message.ConversationId, ct);

        if (conversation is null ||
            (conversation.BuyerUserId != request.UserId && conversation.AgentUserId != request.UserId))
        {
            // Don't leak whether the message exists for other users.
            return ApplicationErrors.Conversation.MessageNotFound;
        }

        if (message.SenderId == request.UserId)
        {
            return ApplicationErrors.Conversation.MessageReadNotAllowed;
        }

        if (!message.MarkAsRead())
            return true;

        await context.SaveChangesAsync(ct);

        return true;
    }
}
