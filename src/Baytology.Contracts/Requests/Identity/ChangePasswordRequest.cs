namespace Baytology.Contracts.Requests.Identity;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
