using System.Net.Http.Json;
using System.Text.Json;

namespace Baytology.Api.Tests.Infrastructure;

internal static class JsonResponseExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<JsonElement>(SerializerOptions);
    }
}
