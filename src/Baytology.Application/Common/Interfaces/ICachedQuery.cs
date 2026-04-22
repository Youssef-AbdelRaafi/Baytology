namespace Baytology.Application.Common.Interfaces;

public interface ICachedQuery<out TResponse>
{
    string CacheKey { get; }

    IEnumerable<string> Tags { get; }

    TimeSpan? Expiration { get; }

    TimeSpan? LocalCacheExpiration { get; }
}
