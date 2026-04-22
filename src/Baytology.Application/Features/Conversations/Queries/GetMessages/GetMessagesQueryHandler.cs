using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Conversations.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Queries.GetMessages;

public class GetMessagesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetMessagesQuery, Result<List<MessageDto>>>
{
    public async Task<Result<List<MessageDto>>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var conversation = await context.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, ct);

        if (conversation is null)
            return Domain.Conversations.ConversationErrors.NotFound;

        if (conversation.BuyerUserId != request.UserId && conversation.AgentUserId != request.UserId)
            return Domain.Conversations.ConversationErrors.Unauthorized;

        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto(
                m.Id, m.ConversationId, m.SenderId, m.Content,
                m.AttachmentUrl, m.IsRead, m.SentAt, m.ReadAt))
            .ToListAsync(ct);

        return messages;
    }
}
