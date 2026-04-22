namespace Baytology.Application.Features.Identity.Dtos;

public sealed record ExternalLoginResultDto(
    AppUserDto User,
    bool IsNewUser);
