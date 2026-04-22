using System.Net.Http.Json;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.Identity;

public class ExternalLoginTokenValidator(
    HttpClient httpClient,
    IOptions<GoogleAuthSettings> googleOptions,
    IOptions<FacebookAuthSettings> facebookOptions,
    ILogger<ExternalLoginTokenValidator> logger) : IExternalLoginTokenValidator
{
    private readonly GoogleAuthSettings _googleSettings = googleOptions.Value;
    private readonly FacebookAuthSettings _facebookSettings = facebookOptions.Value;

    public async Task<Result<ExternalUserInfoDto>> ValidateTokenAsync(string provider, string idToken)
    {
        if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            return await ValidateGoogleTokenAsync(idToken);
        }

        if (provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
        {
            return await ValidateFacebookTokenAsync(idToken);
        }

        return ApplicationErrors.ExternalLogin.ProviderInvalid;
    }

    private async Task<Result<ExternalUserInfoDto>> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            // Google endpoint for token info
            var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
            if (!response.IsSuccessStatusCode)
            {
                return ApplicationErrors.ExternalLogin.GoogleTokenInvalid;
            }

            var payload = await response.Content.ReadFromJsonAsync<GoogleTokenPayload>();
            if (payload == null)
            {
                return ApplicationErrors.ExternalLogin.GoogleTokenParseFailed;
            }

            // Explicit validation
            if (payload.Aud != _googleSettings.ClientId)
            {
                return ApplicationErrors.ExternalLogin.AudienceMismatch;
            }

            if (payload.Iss != "accounts.google.com" && payload.Iss != "https://accounts.google.com")
            {
                return ApplicationErrors.ExternalLogin.GoogleIssuerMismatch;
            }

            if (long.TryParse(payload.Exp, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expirationTime < DateTimeOffset.UtcNow)
                {
                    return ApplicationErrors.ExternalLogin.GoogleTokenExpired;
                }
            }

            return new ExternalUserInfoDto(payload.Sub, payload.Email, payload.GivenName, payload.FamilyName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Google token");
            return ApplicationErrors.ExternalLogin.TokenValidationFailed("Google");
        }
    }

    private async Task<Result<ExternalUserInfoDto>> ValidateFacebookTokenAsync(string accessToken)
    {
        try
        {
            // Facebook app access token formula: {app-id}|{app-secret}
            var appAccessToken = $"{_facebookSettings.AppId}|{_facebookSettings.AppSecret}";

            // First, debug the token to verify it belongs to our app and is valid
            var debugResponse = await httpClient.GetAsync($"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appAccessToken}");
            
            if (!debugResponse.IsSuccessStatusCode)
            {
                return ApplicationErrors.ExternalLogin.FacebookTokenInvalid;
            }

            var debugResult = await debugResponse.Content.ReadFromJsonAsync<FacebookDebugResponse>();
            if (debugResult?.Data == null || !debugResult.Data.IsValid)
            {
                 return ApplicationErrors.ExternalLogin.FacebookTokenNotValid;
            }

            if (debugResult.Data.AppId != _facebookSettings.AppId)
            {
                return ApplicationErrors.ExternalLogin.AudienceMismatch;
            }

            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(debugResult.Data.ExpiresAt);
            if (expirationTime < DateTimeOffset.UtcNow)
            {
                return ApplicationErrors.ExternalLogin.FacebookTokenExpired;
            }

            // Now, get the user info
            var meResponse = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,email,first_name,last_name&access_token={accessToken}");
            if (!meResponse.IsSuccessStatusCode)
            {
                return ApplicationErrors.ExternalLogin.UserInfoFailed;
            }

            var userInfo = await meResponse.Content.ReadFromJsonAsync<FacebookUserInfo>();
            if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
            {
                 return ApplicationErrors.ExternalLogin.EmailMissing;
            }

            return new ExternalUserInfoDto(userInfo.Id, userInfo.Email, userInfo.FirstName, userInfo.LastName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Facebook token");
            return ApplicationErrors.ExternalLogin.TokenValidationFailed("Facebook");
        }
    }

    private class GoogleTokenPayload
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Aud { get; set; } = string.Empty;
        public string Iss { get; set; } = string.Empty;
        public string Exp { get; set; } = string.Empty;
        
        [System.Text.Json.Serialization.JsonPropertyName("given_name")]
        public string? GivenName { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }

    private class FacebookDebugResponse
    {
        public FacebookDebugData Data { get; set; } = null!;
    }

    private class FacebookDebugData
    {
        [System.Text.Json.Serialization.JsonPropertyName("app_id")]
        public string AppId { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }

    private class FacebookUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("last_name")]
        public string? LastName { get; set; }
    }
}
