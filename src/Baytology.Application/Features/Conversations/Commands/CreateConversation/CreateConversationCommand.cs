using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Conversations;
using Baytology.Domain.Properties;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Commands.CreateConversation;

public record CreateConversationCommand(Guid PropertyId, string BuyerUserId)
    : IRequest<Result<Guid>>;
