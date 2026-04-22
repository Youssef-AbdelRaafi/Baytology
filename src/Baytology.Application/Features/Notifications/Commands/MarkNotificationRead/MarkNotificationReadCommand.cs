using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId, string UserId) : IRequest<Result<bool>>;
