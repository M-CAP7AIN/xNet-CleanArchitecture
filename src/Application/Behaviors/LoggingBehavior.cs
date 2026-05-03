using Domain.Interfaces;
using MediatR;
using System.Diagnostics;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly ILoggerService _logger;
        private readonly Stopwatch _stopwatch;

        public LoggingBehavior(ILoggerService logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInfo("Processing {RequestName}: {@Request}", requestName, request);

            _stopwatch.Start();

            try
            {
                var response = await next();
                _stopwatch.Stop();

                _logger.LogMetric("HandlerExecutionTime", _stopwatch.ElapsedMilliseconds,
                    new Dictionary<string, object> { ["RequestName"] = requestName });

                _logger.LogInfo("Completed {RequestName} in {ElapsedMs}ms", requestName, _stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {RequestName}", requestName);
                throw;
            }
        }
    }
}
