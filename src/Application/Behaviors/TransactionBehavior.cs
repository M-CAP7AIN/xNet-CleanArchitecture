using Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IApplicationDbContext _context;

        public TransactionBehavior(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // فقط برای Commands (نوشتن) تراکنش بگیر
            if (IsCommand(request))
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database
                        .BeginTransactionAsync(cancellationToken);

                    try
                    {
                        var response = await next();
                        await transaction.CommitAsync(cancellationToken);
                        return response;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                });
            }

            // برای Queries (خواندن) تراکنش نمی‌گیریم
            return await next();
        }

        private bool IsCommand(TRequest request)
        {
            // Commands معمولاً با Create, Update, Delete, Send شروع می‌شوند
            var requestType = typeof(TRequest).Name;
            return requestType.StartsWith("Create") ||
                   requestType.StartsWith("Update") ||
                   requestType.StartsWith("Delete") ||
                   requestType.EndsWith("Command");
        }
    }
}
