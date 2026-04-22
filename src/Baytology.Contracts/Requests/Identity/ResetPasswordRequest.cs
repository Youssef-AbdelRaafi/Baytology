namespace Baytology.Contracts.Requests.Identity;

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);
