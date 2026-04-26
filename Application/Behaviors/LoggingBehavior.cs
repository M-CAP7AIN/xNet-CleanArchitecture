using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Starting request {RequestName} [ID: {RequestId}] at {StartTime}",
                requestName, requestId, DateTime.UtcNow);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                stopwatch.Stop();

                _logger.LogInformation(
                    "Completed request {RequestName} [ID: {RequestId}] in {ElapsedMilliseconds}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Failed request {RequestName} [ID: {RequestId}] after {ElapsedMilliseconds}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
