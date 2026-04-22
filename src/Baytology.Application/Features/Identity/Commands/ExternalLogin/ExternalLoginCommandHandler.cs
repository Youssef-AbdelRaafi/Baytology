using Baytology.Application.Common.Interfaces;
using Baytology.Domain.AgentDetails;
using Baytology.Domain.Common.Results;
using Baytology.Domain.UserProfiles;

using MediatR;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Features.Identity.Commands.ExternalLogin;

public class ExternalLoginCommandHandler(
    ILogger<ExternalLoginCommandHandler> logger,
    IExternalLoginTokenValidator externalLoginTokenValidator,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IAppDbContext context) : IRequestHandler<ExternalLoginCommand, Result<ExternalLoginCommandResponse>>
{
    public async Task<Result<ExternalLoginCommandResponse>> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var userInfoResult = await externalLoginTokenValidator.ValidateTokenAsync(request.Provider, request.IdToken);
        if (userInfoResult.IsError)
        {
            return userInfoResult.Errors;
        }

        var userInfo = userInfoResult.Value;

        var loginResult = await identityService.ExternalLoginAsync(
            request.Provider,
            userInfo.ProviderSubjectId,
            userInfo.Email,
            userInfo.FirstName,
            userInfo.LastName);

        if (loginResult.IsError)
        {
            return loginResult.Errors;
        }

        var (appUser, isNewUser) = loginResult.Value;

        if (isNewUser)
        {
            var displayName = string.IsNullOrWhiteSpace(userInfo.FirstName) 
                ? userInfo.Email.Split('@')[0] 
                : $"{userInfo.FirstName} {userInfo.LastName}".Trim();

            var userProfileResult = UserProfile.Create(appUser.UserId, displayName);
            if (userProfileResult.IsSuccess)
            {
                context.UserProfiles.Add(userProfileResult.Value);
            }

            // External logins get Buyer role by default. Admins can upgrade them.
            await context.SaveChangesAsync(ct);
        }

        var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(appUser, ct);
        if (generateTokenResult.IsError)
        {
            logger.LogError("External token generation error: {ErrorDescription}", generateTokenResult.TopError.Description);
            return generateTokenResult.Errors;
        }

        return new ExternalLoginCommandResponse(generateTokenResult.Value, isNewUser, appUser.UserId);
    }
}
