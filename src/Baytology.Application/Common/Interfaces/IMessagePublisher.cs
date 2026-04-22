namespace Baytology.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message, CancellationToken ct = default) where T : class;
}
