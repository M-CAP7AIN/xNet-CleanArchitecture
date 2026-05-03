using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IMessageBus
    {
        /// <summary>
        /// انتشار پیام در صف پیش‌فرض یا بر اساس routing key
        /// </summary>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// انتشار پیام در صف خاص با استفاده از routing key
        /// </summary>
        Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
    }
}
