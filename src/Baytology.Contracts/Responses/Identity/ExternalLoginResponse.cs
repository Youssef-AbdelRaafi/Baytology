namespace Baytology.Contracts.Responses.Identity;

public sealed record ExternalLoginResponse(
    TokenResponse Tokens,
    bool IsNewUser,
    string UserId);
