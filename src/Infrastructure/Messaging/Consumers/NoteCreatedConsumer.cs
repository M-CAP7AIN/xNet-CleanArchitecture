using Domain.Events;
using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging.Consumers
{
    public class NoteCreatedConsumer : BackgroundService
    {
        private readonly ILogger<NoteCreatedConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqSettings _settings;
        private readonly IRabbitMqConnectionManager _connectionManager;
        private IChannel? _channel;

        public NoteCreatedConsumer(
            ILogger<NoteCreatedConsumer> logger,
            IServiceProvider serviceProvider,
            IOptions<RabbitMqSettings> settings,
            IRabbitMqConnectionManager connectionManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _settings = settings.Value;
            _connectionManager = connectionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var connection = await _connectionManager.GetConnectionAsync(stoppingToken);
            _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(0, 1, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var noteEvent = JsonSerializer.Deserialize<NoteCreatedEvent>(messageJson);

                    if (noteEvent != null)
                    {
                        await ProcessMessageAsync(noteEvent, args.DeliveryTag, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("Received invalid message");
                        await _channel!.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    await _channel!.BasicNackAsync(args.DeliveryTag, false, true, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("NoteCreatedConsumer started. Queue: {QueueName}", _settings.QueueName);
        }

        private async Task ProcessMessageAsync(NoteCreatedEvent noteEvent, ulong deliveryTag, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

            await cacheService.RemoveAsync("notes:all", cancellationToken);

            _logger.LogInformation("NoteCreatedEvent processed. NoteId: {NoteId}, Title: {Title}",
                noteEvent.NoteId, noteEvent.Title);

            await _channel!.BasicAckAsync(deliveryTag, false, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("NoteCreatedConsumer is stopping.");

            if (_channel is not null)
                await _channel.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
