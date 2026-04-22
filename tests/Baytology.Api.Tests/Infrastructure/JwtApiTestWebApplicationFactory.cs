using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Baytology.Api.Tests.Infrastructure;

public sealed class JwtApiTestWebApplicationFactory : ApiTestWebApplicationFactory
{
    protected override bool UseTestAuthentication => false;

    public async Task<string> CreateAccessTokenAsync(string email, string password)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/api/identity/token/generate", new
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = payload.GetProperty("accessToken").GetString();

        return !string.IsNullOrWhiteSpace(accessToken)
            ? accessToken
            : throw new InvalidOperationException("The identity endpoint returned an empty access token.");
    }
}
