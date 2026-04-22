using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.Conversations.Commands.CreateConversation;
using Baytology.Application.Features.Conversations.Commands.MarkMessageRead;
using Baytology.Application.Features.Conversations.Commands.SendMessage;
using Baytology.Application.Features.Conversations.Dtos;
using Baytology.Application.Features.Conversations.Queries.GetConversations;
using Baytology.Application.Features.Conversations.Queries.GetMessages;
using Baytology.Contracts.Requests.Conversations;
using Baytology.Contracts.Responses.Conversations;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public class ConversationsController(ISender sender) : ApiController
{
    [HttpGet]
    [EndpointSummary("Get all conversations for the current user")]
    [ProducesResponseType(typeof(List<ConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Retrieves all conversations for the current authenticated user (buyer or agent).")]
    [EndpointName("GetConversations")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetConversationsQuery(userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{conversationId:guid}/messages")]
    [EndpointSummary("Get messages for a conversation")]
    [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Retrieves all messages in a conversation for the current user (only if the user is a participant).")]
    [EndpointName("GetConversationMessages")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetMessages(Guid conversationId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetMessagesQuery(conversationId, userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EndpointSummary("Create a new conversation")]
    [ProducesResponseType(typeof(CreateConversationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates a new conversation for the authenticated buyer with the agent owning the selected property.")]
    [EndpointName("CreateConversation")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreateConversationCommand(request.PropertyId, userId);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(id => Ok(new CreateConversationResponse(id)), Problem);
    }

    [HttpPost("{conversationId:guid}/messages")]
    [EndpointSummary("Send a message in a conversation")]
    [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Sends a message to the other participant in the conversation. The sender must be a participant.")]
    [EndpointName("SendMessage")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new SendMessageCommand(conversationId, userId, request.Content, request.AttachmentUrl), ct);
        return result.Match(id => Ok(new SendMessageResponse(id)), Problem);
    }

    [HttpPatch("messages/{messageId:guid}/read")]
    [EndpointSummary("Mark a message as read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Marks the message as read for the authenticated user.")]
    [EndpointName("MarkMessageRead")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> MarkRead(Guid messageId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new MarkMessageReadCommand(messageId, userId), ct);
        return result.Match(_ => Ok(), Problem);
    }
}
