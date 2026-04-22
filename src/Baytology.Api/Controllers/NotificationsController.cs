using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.Notifications.Commands.MarkNotificationRead;
using Baytology.Application.Features.Notifications.Queries.GetNotifications;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public class NotificationsController(ISender sender) : ApiController
{
    [HttpGet]
    [EndpointSummary("Get notifications for the current user")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Gets notifications for the authenticated user. Supports unread-only filtering via query parameter.")]
    [EndpointName("GetNotifications")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetNotificationsQuery(userId, unreadOnly), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPatch("{id:guid}/read")]
    [EndpointSummary("Mark a notification as read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Marks a notification as read for the authenticated user.")]
    [EndpointName("MarkNotificationRead")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new MarkNotificationReadCommand(id, userId), ct);
        return result.Match(_ => Ok(), Problem);
    }
}
