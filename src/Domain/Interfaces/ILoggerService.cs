using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface ILoggerService
    {
        void LogInfo(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// شروع یک عملیات با Scope (برای ثبت CorrelationId)
        /// </summary>
        IDisposable BeginScope(string operationName, params object[] args);

        /// <summary>
        /// ثبت متریک (عملکرد)
        /// </summary>
        void LogMetric(string name, double value, Dictionary<string, object>? tags = null);
    }
}
