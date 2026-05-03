using Domain.Interfaces;
using Domain.Settings;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging
{
    public class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly IRabbitMqConnectionManager _connectionManager;
        private readonly ILogger<RabbitMqMessageBus> _logger;
        private IChannel? _channel;
        private bool _disposed;

        public RabbitMqMessageBus(
            IOptions<RabbitMqSettings> settings,
            IRabbitMqConnectionManager connectionManager,
            ILogger<RabbitMqMessageBus> logger)
        {
            _settings = settings.Value;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare Exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Declare Queue
            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            // Bind Queue to Exchange
            await _channel.QueueBindAsync(
                queue: _settings.QueueName,
                exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey,
                cancellationToken: cancellationToken);

            return _channel;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            await PublishAsync(message, _settings.RoutingKey, cancellationToken);
        }

        public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            try
            {
                var channel = await GetChannelAsync(cancellationToken);

                var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await channel.BasicPublishAsync(
                    exchange: _settings.ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: messageBody,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Message published successfully. RoutingKey: {RoutingKey}, Type: {MessageType}",
                    routingKey, typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message. RoutingKey: {RoutingKey}, Type: {MessageType}",
                    routingKey, typeof(T).Name);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            if (_channel is not null)
                await _channel.CloseAsync();
        }
    }
}
