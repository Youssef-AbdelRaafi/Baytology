namespace Baytology.Contracts.Requests.Identity;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string DisplayName,
    string Role = "Buyer");
