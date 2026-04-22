using System.Security.Claims;

using Baytology.Application.Features.Identity;
using Baytology.Application.Features.Identity.Dtos;
using Baytology.Domain.Common.Results;

namespace Baytology.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
