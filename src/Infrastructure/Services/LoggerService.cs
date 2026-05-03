using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public IDisposable BeginScope(string operationName, params object[] args)
        {
            return _logger.BeginScope(new Dictionary<string, object>
            {
                [operationName] = args
            });
        }

        public void LogMetric(string name, double value, Dictionary<string, object>? tags = null)
        {
            var metricData = new Dictionary<string, object>
            {
                ["MetricName"] = name,
                ["Value"] = value
            };

            if (tags != null)
            {
                foreach (var tag in tags)
                    metricData[tag.Key] = tag.Value;
            }

            _logger.LogInformation("Metric: {MetricName}, Value: {Value}, {@Tags}",
                name, value, tags);
        }
    }
}
