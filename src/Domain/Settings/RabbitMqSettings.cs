using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Settings
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string QueueName { get; set; } = "notes_queue";
        public string ExchangeName { get; set; } = "notes_exchange";
        public string RoutingKey { get; set; } = "note.created";
        public int RetryCount { get; set; } = 5;
        public int RetryInitialDelayMs { get; set; } = 1000;
    }
}
