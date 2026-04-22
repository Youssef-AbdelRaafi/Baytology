using Baytology.Domain.Common.Results;

namespace Baytology.Application.Common.Interfaces;

public record ExternalUserInfoDto(
    string ProviderSubjectId,
    string Email,
    string? FirstName,
    string? LastName);

public interface IExternalLoginTokenValidator
{
    Task<Result<ExternalUserInfoDto>> ValidateTokenAsync(string provider, string idToken);
}
