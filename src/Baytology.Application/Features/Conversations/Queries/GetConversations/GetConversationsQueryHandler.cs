using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Conversations.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Queries.GetConversations;

public class GetConversationsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetConversationsQuery, Result<List<ConversationDto>>>
{
    public async Task<Result<List<ConversationDto>>> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var conversations = await context.Conversations
            .AsNoTracking()
            .Where(c => c.BuyerUserId == request.UserId || c.AgentUserId == request.UserId)
            .OrderByDescending(c => c.LastMessageAt)
            .Select(c => new ConversationDto(
                c.Id, c.PropertyId, c.BuyerUserId, c.AgentUserId,
                context.UserProfiles.Where(p => p.UserId == c.BuyerUserId).Select(p => p.DisplayName).FirstOrDefault(),
                context.UserProfiles.Where(p => p.UserId == c.AgentUserId).Select(p => p.DisplayName).FirstOrDefault(),
                context.Properties.Where(p => p.Id == c.PropertyId).Select(p => p.Title).FirstOrDefault(),
                c.CreatedOnUtc, c.LastMessageAt,
                context.Messages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return conversations;
    }
}
