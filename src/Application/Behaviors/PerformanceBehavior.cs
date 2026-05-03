using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
    {
        private readonly ILoggerService _logger;
        private readonly long _thresholdMilliseconds = 500;

        public PerformanceBehavior(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestName = typeof(TRequest).Name;

            try
            {
                _logger.LogDebug("Processing request: {RequestName}", requestName);

                var response = await next();

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                if (elapsedMilliseconds > _thresholdMilliseconds)
                {
                    _logger.LogWarning(
                        "Long Running Request: {RequestName} ({ElapsedMilliseconds} ms) {@Request}",
                        requestName, elapsedMilliseconds, request);
                }
                else
                {
                    _logger.LogDebug("Request completed: {RequestName} ({ElapsedMilliseconds} ms)",
                        requestName, elapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request: {RequestName}", requestName);
                throw;
            }
        }
    }
}
