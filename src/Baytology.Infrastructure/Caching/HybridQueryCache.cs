using Baytology.Application.Common.Interfaces;

using Microsoft.Extensions.Caching.Hybrid;

namespace Baytology.Infrastructure.Caching;

internal sealed class HybridQueryCache(HybridCache cache) : IQueryCache
{
    public ValueTask<TResponse> GetOrCreateAsync<TResponse>(
        string key,
        Func<CancellationToken, ValueTask<TResponse>> factory,
        TimeSpan? expiration,
        TimeSpan? localCacheExpiration,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken = default)
    {
        var options = BuildEntryOptions(expiration, localCacheExpiration);
        var normalizedTags = Normalize(tags);

        return cache.GetOrCreateAsync(
            key,
            factory,
            options,
            normalizedTags,
            cancellationToken);
    }

    public ValueTask RemoveByTagAsync(IEnumerable<string>? tags, CancellationToken cancellationToken = default)
    {
        var normalizedTags = Normalize(tags);

        if (normalizedTags.Length == 0)
            return ValueTask.CompletedTask;

        return cache.RemoveByTagAsync(normalizedTags, cancellationToken);
    }

    private static HybridCacheEntryOptions? BuildEntryOptions(TimeSpan? expiration, TimeSpan? localCacheExpiration)
    {
        if (!expiration.HasValue && !localCacheExpiration.HasValue)
            return null;

        return new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = localCacheExpiration
        };
    }

    private static string[] Normalize(IEnumerable<string>? values)
    {
        return values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? [];
    }
}
