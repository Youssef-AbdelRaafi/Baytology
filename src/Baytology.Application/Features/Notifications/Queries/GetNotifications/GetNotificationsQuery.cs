using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(string UserId, bool UnreadOnly = false) : IRequest<Result<List<NotificationDto>>>;

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTimeOffset CreatedOnUtc);
