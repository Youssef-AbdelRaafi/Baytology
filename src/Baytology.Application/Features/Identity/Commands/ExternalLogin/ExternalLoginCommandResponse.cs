namespace Baytology.Application.Features.Identity.Commands.ExternalLogin;

public record ExternalLoginCommandResponse(
    TokenResponse Tokens,
    bool IsNewUser,
    string UserId);
