using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;

namespace Application.Behaviors
{
    public interface IAuthorizedRequest
    {
        string RequiredRole { get; }
    }

    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is IAuthorizedRequest authorizedRequest)
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                    throw new UnauthorizedAccessException("User is not authenticated");

                if (!string.IsNullOrEmpty(authorizedRequest.RequiredRole))
                {
                    if (!user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == authorizedRequest.RequiredRole))
                        throw new UnauthorizedAccessException($"User does not have required role: {authorizedRequest.RequiredRole}");
                }
            }

            return await next();
        }
    }
}
