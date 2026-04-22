namespace Baytology.Contracts.Requests.Identity;

public sealed record ConfirmEmailRequest(
    string UserId,
    string Token);
