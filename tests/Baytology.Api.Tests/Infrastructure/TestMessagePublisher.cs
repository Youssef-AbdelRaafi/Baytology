using Baytology.Application.Common.Interfaces;

namespace Baytology.Api.Tests.Infrastructure;

internal sealed class TestMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<T>(string queue, T message, CancellationToken ct = default) where T : class
    {
        return Task.CompletedTask;
    }
}
