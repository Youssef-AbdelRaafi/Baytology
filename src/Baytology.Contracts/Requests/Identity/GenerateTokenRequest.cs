namespace Baytology.Contracts.Requests.Identity;

public sealed record GenerateTokenRequest(
    string Email,
    string Password);
