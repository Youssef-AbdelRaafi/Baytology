using Baytology.Application.Common.Interfaces;
using Baytology.Application.Features.Recommendations.EventHandlers;
using Baytology.Domain.Common.Enums;
using Baytology.Domain.Recommendations.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Baytology.Application.Tests.Recommendations;

public sealed class RecommendationRequestedEventHandlerTests
{
    [Fact]
    public async Task Recommendation_requested_handler_does_not_throw_when_internal_fallback_fails()
    {
        using var services = new ServiceCollection()
            .AddScoped<IAiDispatchPolicy>(_ => new StaticDispatchPolicy())
            .AddScoped<IRecommendationFallbackService>(_ => new ThrowingRecommendationFallbackService())
            .BuildServiceProvider();

        var handler = new RecommendationRequestedEventHandler(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<RecommendationRequestedEventHandler>.Instance);

        await handler.Handle(
            new RecommendationRequestedEvent(Guid.NewGuid(), "buyer-1", "Property", Guid.NewGuid().ToString("D"), 5, null),
            CancellationToken.None);
    }

    private sealed class StaticDispatchPolicy : IAiDispatchPolicy
    {
        public bool ShouldDeferSearchResolution(SearchInputType inputType) => false;
        public bool ShouldDeferRecommendationResolution() => false;
    }

    private sealed class ThrowingRecommendationFallbackService : IRecommendationFallbackService
    {
        public Task<RecommendationFallbackResolution?> ResolveAsync(Guid recommendationRequestId, CancellationToken ct = default)
            => throw new InvalidOperationException("Recommendation fallback is unavailable.");
    }
}
