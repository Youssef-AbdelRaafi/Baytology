using System.Security.Claims;

namespace Baytology.Application.Features.Identity.Dtos;

public sealed record AppUserDto(string UserId, string Email, IList<string> Roles, IList<Claim> Claims, string? DisplayName = null);
