using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;

namespace Application.Behaviors
{
    public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILoggerService _logger;
        private readonly int _retryCount = 3;

        public RetryBehavior(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var retryPolicy = Policy
                .Handle<Exception>(ex => IsTransientException(ex))
                .WaitAndRetryAsync(
                    _retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception.Message,
                            "Retry {RetryCount}/{TotalRetries} for request {RequestName} after {Delay}ms",
                            retryCount, _retryCount, typeof(TRequest).Name, timeSpan.TotalMilliseconds);
                    });

            return await retryPolicy.ExecuteAsync(async () => await next());
        }

        private bool IsTransientException(Exception ex)
        {
            // خطاهای موقتی مانند مشکل شبکه، timeout دیتابیس و...
            return ex is TimeoutException ||
                   ex.Message.Contains("timeout") ||
                   ex.Message.Contains("deadlock") ||
                   ex.Message.Contains("connection");
        }
    }
}
