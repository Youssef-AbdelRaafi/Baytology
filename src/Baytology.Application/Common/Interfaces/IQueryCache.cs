namespace Baytology.Application.Common.Interfaces;

public interface IQueryCache
{
    ValueTask<TResponse> GetOrCreateAsync<TResponse>(
        string key,
        Func<CancellationToken, ValueTask<TResponse>> factory,
        TimeSpan? expiration,
        TimeSpan? localCacheExpiration,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(IEnumerable<string>? tags, CancellationToken cancellationToken = default);
}
