using System.Text;
using System.Text.Json;

using Baytology.Application.Common.Interfaces;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace Baytology.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _initialized;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task InitializeAsync()
    {
        if (!_settings.Enabled)
        {
            _initialized = true;
            return;
        }

        if (_initialized && _channel is not null)
            return;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _initialized = true;

            _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", _settings.HostName, _settings.Port);
        }
        catch (Exception ex)
        {
            _initialized = false;
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}. Outbox delivery will be retried.", _settings.HostName, _settings.Port);
            throw new InvalidOperationException("RabbitMQ publishing is currently unavailable.", ex);
        }
    }

    public async Task PublishAsync<T>(string queue, T message, CancellationToken ct = default) where T : class
    {
        await InitializeAsync();

        if (!_settings.Enabled)
        {
            _logger.LogInformation("RabbitMQ publishing is disabled. Skipping message for queue {Queue}.", queue);
            return;
        }

        var json = JsonSerializer.Serialize(message, SerializerOptions);

        try
        {
            if (_channel is null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(json);
            await _channel.BasicPublishAsync("", queue, body, ct);

            _logger.LogInformation("Published message to queue {Queue}", queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {Queue}", queue);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}
