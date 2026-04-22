using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.Identity;
using Baytology.Application.Features.Identity.Commands.RegisterUser;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Application.Features.Identity.Queries.GenerateTokens;
using Baytology.Application.Features.Identity.Queries.GetUserInfo;
using Baytology.Application.Features.Identity.Queries.RefreshTokens;
using Baytology.Contracts.Requests.Identity;
using Baytology.Contracts.Responses.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/identity")]
[Route("api/v{version:apiVersion}/identity")]
public sealed class IdentityController(ISender sender) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Register a new user")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Registers a new user (Buyer or Agent) and creates their initial profile.")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var command = new RegisterUserCommand(request.Email, request.Password, request.DisplayName, request.Role);
        var result = await sender.Send(command, ct);
        return result.Match(id => Ok(new RegisterUserResponse(id)), Problem);
    }

    [HttpPost("token/generate")]
    [ProducesResponseType(typeof(Baytology.Contracts.Responses.Identity.TokenResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Generate access and refresh tokens")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Validates email/password and returns a new access token and refresh token.")]
    public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GenerateTokenQuery(request.Email, request.Password), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("token/refresh")]
    [ProducesResponseType(typeof(Baytology.Contracts.Responses.Identity.TokenResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Refresh expired access token")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Validates refresh token and an expired access token, then returns a new access token.")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RefreshTokenQuery(request.RefreshToken, request.ExpiredAccessToken), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [EndpointSummary("Get current authenticated user info")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Returns the current authenticated user profile, including roles and claims.")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetUserByIdQuery(userId), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost("external-login")]
    [ProducesResponseType(typeof(ExternalLoginResponse), StatusCodes.Status200OK)]
    [EndpointSummary("Login or register via external provider (Google/Facebook)")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequest request, CancellationToken ct)
    {
        var command = new Baytology.Application.Features.Identity.Commands.ExternalLogin.ExternalLoginCommand(request.Provider, request.IdToken);
        var result = await sender.Send(command, ct);
        return result.Match(response => Ok(new ExternalLoginResponse(
            new Baytology.Contracts.Responses.Identity.TokenResponse(response.Tokens.AccessToken, response.Tokens.RefreshToken, response.Tokens.ExpiresOnUtc),
            response.IsNewUser,
            response.UserId)), Problem);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Change current user's password")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new Baytology.Application.Features.Identity.Commands.ChangePassword.ChangePasswordCommand(userId!, request.CurrentPassword, request.NewPassword);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Request a password reset email")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var command = new Baytology.Application.Features.Identity.Commands.ForgotPassword.ForgotPasswordCommand(request.Email);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Reset password using a token")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var command = new Baytology.Application.Features.Identity.Commands.ResetPassword.ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Confirm email using a token")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    {
        var command = new Baytology.Application.Features.Identity.Commands.ConfirmEmail.ConfirmEmailCommand(request.UserId, request.Token);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Resend confirmation email")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken ct)
    {
        var command = new Baytology.Application.Features.Identity.Commands.ResendConfirmation.ResendConfirmationCommand(request.Email);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Revoke refresh tokens for the current user")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new Baytology.Application.Features.Identity.Commands.Logout.LogoutCommand(userId!);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpDelete("account")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Soft delete the current user account")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new Baytology.Application.Features.Identity.Commands.DeleteAccount.DeleteAccountCommand(userId!);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }
}
