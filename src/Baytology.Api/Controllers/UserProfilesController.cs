using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.UserProfiles.Commands.CreateUserProfile;
using Baytology.Application.Features.UserProfiles.Commands.UpdateUserProfile;
using Baytology.Application.Features.UserProfiles.Dtos;
using Baytology.Application.Features.UserProfiles.Queries.GetUserProfile;
using Baytology.Contracts.Requests.UserProfiles;
using Baytology.Contracts.Responses.UserProfiles;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public class UserProfilesController(ISender sender) : ApiController
{
    [HttpGet("{userId}")]
    [EndpointSummary("Get user profile by user ID")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Gets a user profile by its user id.")]
    [EndpointName("GetUserProfile")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetProfile(string userId, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserProfileQuery(userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("me")]
    [EndpointSummary("Get current user profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Gets the user profile for the current authenticated user.")]
    [EndpointName("GetMyUserProfile")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetUserProfileQuery(userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EndpointSummary("Create user profile")]
    [ProducesResponseType(typeof(CreateUserProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates a user profile for the authenticated user. UserId is derived from the JWT.")]
    [EndpointName("CreateUserProfile")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreateUserProfileCommand(
            userId,
            request.DisplayName,
            request.AvatarUrl,
            request.Bio,
            request.PhoneNumber,
            (Baytology.Domain.Common.Enums.ContactMethod)request.PreferredContactMethod);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(
            id => CreatedAtAction(nameof(GetProfile), new { userId }, new CreateUserProfileResponse(id)),
            Problem);
    }

    [HttpPut("me")]
    [EndpointSummary("Update current user profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Updates the authenticated user's profile information.")]
    [EndpointName("UpdateUserProfile")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new UpdateUserProfileCommand(
            userId,
            request.DisplayName,
            request.AvatarUrl,
            request.Bio,
            request.PhoneNumber,
            (Baytology.Domain.Common.Enums.ContactMethod)request.PreferredContactMethod);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(_ => Ok(), Problem);
    }
}
