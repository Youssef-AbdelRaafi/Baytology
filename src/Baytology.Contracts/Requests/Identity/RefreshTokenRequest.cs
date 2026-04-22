namespace Baytology.Contracts.Requests.Identity;

public sealed record RefreshTokenRequest(
    string RefreshToken,
    string ExpiredAccessToken);
