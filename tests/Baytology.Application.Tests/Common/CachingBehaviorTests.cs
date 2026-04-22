using Baytology.Application.Common.Behaviours;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.Common;

public sealed class CachingBehaviorTests
{
    [Fact]
    public async Task Cached_query_uses_cached_value_on_second_call()
    {
        var cache = new FakeQueryCache();
        var behavior = new CachingBehavior<TestCachedQuery, Result<string>>(cache, NullLogger<CachingBehavior<TestCachedQuery, Result<string>>>.Instance);
        var query = new TestCachedQuery("properties:cairo");
        var invocationCount = 0;

        var first = await behavior.Handle(
            query,
            _ =>
            {
                invocationCount++;
                return Task.FromResult<Result<string>>($"payload-{invocationCount}");
            },
            CancellationToken.None);

        var second = await behavior.Handle(
            query,
            _ =>
            {
                invocationCount++;
                return Task.FromResult<Result<string>>($"payload-{invocationCount}");
            },
            CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal("payload-1", first.Value);
        Assert.Equal("payload-1", second.Value);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Cache_failures_fall_back_to_direct_execution()
    {
        var cache = new ThrowingQueryCache();
        var behavior = new CachingBehavior<TestCachedQuery, Result<string>>(cache, NullLogger<CachingBehavior<TestCachedQuery, Result<string>>>.Instance);
        var query = new TestCachedQuery("properties:missing");
        var invocationCount = 0;

        var result = await behavior.Handle(
            query,
            _ =>
            {
                invocationCount++;
                return Task.FromResult<Result<string>>(Error.NotFound("Property.NotFound", "Property not found."));
            },
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(1, invocationCount);
        Assert.Equal("Property.NotFound", result.TopError.Code);
    }

    private sealed record TestCachedQuery(string CacheKey) : IRequest<Result<string>>, ICachedQuery<Result<string>>
    {
        public IEnumerable<string> Tags => ["properties"];

        public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

        public TimeSpan? LocalCacheExpiration => TimeSpan.FromMinutes(1);
    }

    private sealed class FakeQueryCache : IQueryCache
    {
        private readonly Dictionary<string, object> _items = [];

        public async ValueTask<TResponse> GetOrCreateAsync<TResponse>(
            string key,
            Func<CancellationToken, ValueTask<TResponse>> factory,
            TimeSpan? expiration,
            TimeSpan? localCacheExpiration,
            IEnumerable<string>? tags,
            CancellationToken cancellationToken = default)
        {
            if (_items.TryGetValue(key, out var existing))
                return (TResponse)existing;

            var created = await factory(cancellationToken);
            _items[key] = created!;
            return created;
        }

        public ValueTask RemoveByTagAsync(IEnumerable<string>? tags, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    private sealed class ThrowingQueryCache : IQueryCache
    {
        public ValueTask<TResponse> GetOrCreateAsync<TResponse>(
            string key,
            Func<CancellationToken, ValueTask<TResponse>> factory,
            TimeSpan? expiration,
            TimeSpan? localCacheExpiration,
            IEnumerable<string>? tags,
            CancellationToken cancellationToken = default)
            => ValueTask.FromException<TResponse>(new InvalidOperationException("Cache serialization failed."));

        public ValueTask RemoveByTagAsync(IEnumerable<string>? tags, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
