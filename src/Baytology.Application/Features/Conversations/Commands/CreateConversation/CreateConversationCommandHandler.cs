using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Conversations;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Commands.CreateConversation;

public class CreateConversationCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateConversationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConversationCommand request, CancellationToken ct)
    {
        var property = await context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, ct);

        if (property is null)
            return PropertyErrors.NotFound;

        if (property.AgentUserId == request.BuyerUserId)
            return ApplicationErrors.Conversation.SelfContact;

        var agentProfileExists = await context.AgentDetails
            .AsNoTracking()
            .AnyAsync(a => a.UserId == property.AgentUserId, ct);

        if (!agentProfileExists)
            return ApplicationErrors.Conversation.AgentUnavailable;

        var existingConversationId = await context.Conversations
            .Where(c =>
            c.PropertyId == request.PropertyId &&
            c.BuyerUserId == request.BuyerUserId &&
            c.AgentUserId == property.AgentUserId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        if (existingConversationId.HasValue)
            return existingConversationId.Value;

        var conversationResult = Conversation.Create(request.PropertyId, request.BuyerUserId, property.AgentUserId);
        if (conversationResult.IsError)
            return conversationResult.Errors;

        var conversation = conversationResult.Value;
        context.Conversations.Add(conversation);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var concurrentConversationId = await context.Conversations
                .AsNoTracking()
                .Where(c =>
                    c.PropertyId == request.PropertyId &&
                    c.BuyerUserId == request.BuyerUserId &&
                    c.AgentUserId == property.AgentUserId)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(ct);

            if (concurrentConversationId.HasValue)
                return concurrentConversationId.Value;

            throw;
        }

        return conversation.Id;
    }
}
