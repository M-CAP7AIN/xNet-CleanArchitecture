using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellation = default);
        Task RemoveAsync(string key, CancellationToken cancellation = default);
    }
}
