using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Conversations.Dtos;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Queries.GetMessages;

public record GetMessagesQuery(Guid ConversationId, string UserId) : IRequest<Result<List<MessageDto>>>;
