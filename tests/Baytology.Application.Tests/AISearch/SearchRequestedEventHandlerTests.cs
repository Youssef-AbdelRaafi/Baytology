using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.AISearch.EventHandlers;
using Baytology.Domain.AISearch.Events;
using Baytology.Domain.Common.Enums;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.AISearch;

public sealed class SearchRequestedEventHandlerTests
{
    [Fact]
    public async Task Search_requested_handler_does_not_throw_when_internal_fallback_fails()
    {
        using var services = new ServiceCollection()
            .AddScoped<IAiDispatchPolicy>(_ => new StaticDispatchPolicy())
            .AddScoped<IAiSearchFallbackService>(_ => new ThrowingSearchFallbackService())
            .BuildServiceProvider();

        var handler = new SearchRequestedEventHandler(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<SearchRequestedEventHandler>.Instance);

        await handler.Handle(
            new SearchRequestedEvent(Guid.NewGuid(), "buyer-1", SearchInputType.Text, SearchEngine.Hybrid, null),
            CancellationToken.None);
    }

    private sealed class StaticDispatchPolicy : IAiDispatchPolicy
    {
        public bool ShouldDeferSearchResolution(SearchInputType inputType) => false;
        public bool ShouldDeferRecommendationResolution() => false;
    }

    private sealed class ThrowingSearchFallbackService : IAiSearchFallbackService
    {
        public Task<AiSearchFallbackResolution?> ResolveAsync(Guid searchRequestId, CancellationToken ct = default)
            => throw new InvalidOperationException("Search fallback is unavailable.");
    }
}
