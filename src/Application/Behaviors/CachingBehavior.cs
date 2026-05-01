using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Behaviors
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        int CacheDurationInMinutes { get; }
    }

    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IMemoryCache _cache;

        public CachingBehavior(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // اگر Query قابل کش شدن است
            if (request is ICacheableQuery cacheableQuery)
            {
                var cacheKey = cacheableQuery.CacheKey;

                // تلاش برای گرفتن از کش
                if (_cache.TryGetValue(cacheKey, out TResponse cachedResponse))
                    return cachedResponse;

                // اجرای Handler و ذخیره در کش
                var response = await next();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheableQuery.CacheDurationInMinutes))
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, response, cacheOptions);

                return response;
            }

            return await next();
        }
    }
}
