using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IDapperService
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null);
        Task<int> ExecuteAsync(string sql, object? parameters = null);
    }
}
