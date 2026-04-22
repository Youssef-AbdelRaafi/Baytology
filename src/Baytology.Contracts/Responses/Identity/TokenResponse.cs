namespace Baytology.Contracts.Responses.Identity;

public sealed record TokenResponse(
    string? AccessToken,
    string? RefreshToken,
    DateTime ExpiresOnUtc);
