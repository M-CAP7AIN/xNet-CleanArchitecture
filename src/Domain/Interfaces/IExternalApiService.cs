using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IExternalApiService
    {
        Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null);
        Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null);
        Task<T?> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null);
        Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null);
    }
}
