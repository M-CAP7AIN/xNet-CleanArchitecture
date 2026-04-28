using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default)
        {
            var data = await _cache.GetStringAsync(key, cancellation);
            return data == null ? default : JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellation = default)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry;
            else
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options, cancellation);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellation = default)
        {
            await _cache.RemoveAsync(key, cancellation);
        }
    }
}
