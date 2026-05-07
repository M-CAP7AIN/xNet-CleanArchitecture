using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Interfaces
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerService _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExternalApiService(
            IHttpClientFactory httpClientFactory,
            ILoggerService logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddHeaders(client, headers);

                _logger.LogInfo("Sending GET request to {Url}", url);

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                _logger.LogInfo("GET request to {Url} completed successfully", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET request to {Url} failed", url);
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddHeaders(client, headers);

                var jsonContent = data != null
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;

                _logger.LogInfo("Sending POST request to {Url}", url);

                var response = await client.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                _logger.LogInfo("POST request to {Url} completed successfully", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST request to {Url} failed", url);
                throw;
            }
        }

        public async Task<T?> PutAsync<T>(string url, object? data = null, Dictionary<string, string>? headers = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddHeaders(client, headers);

                var jsonContent = data != null
                    ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                    : null;

                _logger.LogInfo("Sending PUT request to {Url}", url);

                var response = await client.PutAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                _logger.LogInfo("PUT request to {Url} completed successfully", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT request to {Url} failed", url);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddHeaders(client, headers);

                _logger.LogInfo("Sending DELETE request to {Url}", url);

                var response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                _logger.LogInfo("DELETE request to {Url} completed successfully", url);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE request to {Url} failed", url);
                return false;
            }
        }

        private static void AddHeaders(HttpClient client, Dictionary<string, string>? headers)
        {
            client.DefaultRequestHeaders.Clear();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
    }
}
