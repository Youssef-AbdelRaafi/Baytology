namespace Baytology.Contracts.Requests.Identity;

public sealed record ExternalLoginRequest(
    string Provider,
    string IdToken);
