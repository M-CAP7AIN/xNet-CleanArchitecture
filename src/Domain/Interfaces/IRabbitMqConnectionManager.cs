using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IRabbitMqConnectionManager : IAsyncDisposable
    {
        Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
        bool IsConnected { get; }
    }
}
