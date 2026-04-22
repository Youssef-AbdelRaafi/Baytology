using Baytology.Application.Common.Behaviours;
using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;

using MediatR;

namespace Baytology.Application.Tests.Common;

public sealed class CacheInvalidationBehaviorTests
{
    [Fact]
    public async Task Successful_mutation_invalidates_configured_tags()
    {
        var cache = new FakeQueryCache();
        var behavior = new CacheInvalidationBehavior<TestMutationCommand, Result<Success>>(cache);

        var result = await behavior.Handle(
            new TestMutationCommand(),
            _ => Task.FromResult<Result<Success>>(Result.Success),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["properties", "saved-properties"], cache.InvalidatedTags);
    }

    [Fact]
    public async Task Failed_mutation_does_not_invalidate_tags()
    {
        var cache = new FakeQueryCache();
        var behavior = new CacheInvalidationBehavior<TestMutationCommand, Result<Success>>(cache);

        var result = await behavior.Handle(
            new TestMutationCommand(),
            _ => Task.FromResult<Result<Success>>(Error.Validation("Mutation.Failed", "Failure")),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Empty(cache.InvalidatedTags);
    }

    private sealed record TestMutationCommand : IRequest<Result<Success>>, ICacheInvalidationRequest
    {
        public IEnumerable<string> CacheTagsToInvalidate => ["properties", "saved-properties"];
    }

    private sealed class FakeQueryCache : IQueryCache
    {
        public List<string> InvalidatedTags { get; } = [];

        public ValueTask<TResponse> GetOrCreateAsync<TResponse>(
            string key,
            Func<CancellationToken, ValueTask<TResponse>> factory,
            TimeSpan? expiration,
            TimeSpan? localCacheExpiration,
            IEnumerable<string>? tags,
            CancellationToken cancellationToken = default)
            => factory(cancellationToken);

        public ValueTask RemoveByTagAsync(IEnumerable<string>? tags, CancellationToken cancellationToken = default)
        {
            InvalidatedTags.AddRange(tags ?? []);
            return ValueTask.CompletedTask;
        }
    }
}
