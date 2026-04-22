using Baytology.Application.Common.Interfaces;

using MediatR;
using Microsoft.Extensions.Logging;

namespace Baytology.Application.Common.Behaviours;

public sealed class CachingBehavior<TRequest, TResponse>(IQueryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICachedQuery<TResponse> cachedQuery || string.IsNullOrWhiteSpace(cachedQuery.CacheKey))
            return await next(ct);

        try
        {
            return await cache.GetOrCreateAsync(
                cachedQuery.CacheKey,
                token => new ValueTask<TResponse>(next(token)),
                cachedQuery.Expiration,
                cachedQuery.LocalCacheExpiration,
                cachedQuery.Tags,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Cache read/write failed for {RequestType} and key {CacheKey}. Falling back to direct query execution.",
                typeof(TRequest).Name,
                cachedQuery.CacheKey);

            return await next(ct);
        }
    }
}
