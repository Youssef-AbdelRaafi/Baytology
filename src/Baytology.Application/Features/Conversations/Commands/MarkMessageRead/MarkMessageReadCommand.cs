using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Conversations.Commands.MarkMessageRead;

public record MarkMessageReadCommand(Guid MessageId, string UserId) : IRequest<Result<bool>>;
