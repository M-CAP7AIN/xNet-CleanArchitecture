using Domain.Interfaces;
using Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Infrastructure.Messaging
{
    public class RabbitMqConnectionManager : IRabbitMqConnectionManager
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqConnectionManager> _logger;
        private IConnection? _connection;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private bool _disposed;

        public RabbitMqConnectionManager(
            IOptions<RabbitMqSettings> settings,
            ILogger<RabbitMqConnectionManager> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsConnected => _connection is { IsOpen: true };

        public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqConnectionManager));

            if (IsConnected)
                return _connection!;

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (IsConnected)
                    return _connection!;

                var retryPolicy = Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetryAsync(
                        _settings.RetryCount,
                        retryAttempt => TimeSpan.FromMilliseconds(_settings.RetryInitialDelayMs * Math.Pow(2, retryAttempt - 1)),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning(exception,
                                "RabbitMQ connection attempt {RetryCount} failed. Retrying in {Delay}ms...",
                                retryCount, timeSpan.TotalMilliseconds);
                        });

                _connection = await retryPolicy.ExecuteAsync(async () =>
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _settings.HostName,
                        VirtualHost = _settings.VirtualHost,
                        Port = _settings.Port,
                        UserName = _settings.UserName,
                        Password = _settings.Password,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                    };

                    var connection = await factory.CreateConnectionAsync();

                    connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                    connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

                    _logger.LogInformation("RabbitMQ connection established successfully.");
                    return connection;
                });

                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private Task OnConnectionShutdownAsync(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection shutdown. Reason: {Reason}", e.ReplyText);
            return Task.CompletedTask;
        }

        private Task OnConnectionBlockedAsync(object? sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection blocked. Reason: {Reason}", e.Reason);
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            if (_connection is not null)
                await _connection.DisposeAsync();

            _connectionLock.Dispose();
        }
    }
}
