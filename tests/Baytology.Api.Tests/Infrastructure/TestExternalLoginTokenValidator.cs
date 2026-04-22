using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

namespace Baytology.Api.Tests.Infrastructure;

public class TestExternalLoginTokenValidator : IExternalLoginTokenValidator
{
    public Task<Result<ExternalUserInfoDto>> ValidateTokenAsync(string provider, string idToken)
    {
        if (idToken == "invalid-token")
        {
            return Task.FromResult<Result<ExternalUserInfoDto>>(Error.Unauthorized("ExternalLogin_Token_Invalid", "Invalid token"));
        }

        if (idToken == "expired-token")
        {
            return Task.FromResult<Result<ExternalUserInfoDto>>(Error.Unauthorized("ExternalLogin_Token_Expired", "Token has expired"));
        }

        if (idToken == "valid-google-token" && provider == "Google")
        {
            return Task.FromResult<Result<ExternalUserInfoDto>>(new ExternalUserInfoDto("test-google-id", "googleuser@test.local", "Google", "User"));
        }

        if (idToken == "buyer-google-token" && provider == "Google")
        {
            return Task.FromResult<Result<ExternalUserInfoDto>>(new ExternalUserInfoDto("buyer-google-id", TestSeedData.BuyerEmail, "Buyer", "User"));
        }

        if (idToken == "valid-facebook-token" && provider == "Facebook")
        {
            return Task.FromResult<Result<ExternalUserInfoDto>>(new ExternalUserInfoDto("test-facebook-id", "facebookuser@test.local", "Facebook", "User"));
        }

        return Task.FromResult<Result<ExternalUserInfoDto>>(Error.Validation("ExternalLogin_Provider_Invalid", "Unsupported provider or token combination"));
    }
}
