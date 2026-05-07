using Domain.Interfaces;
using System.Security.Claims;

namespace WebApi.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _logger;

        public AuthorizationMiddleware(RequestDelegate next, ILoggerService logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                var authorizeAttribute = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

                if (authorizeAttribute != null)
                {
                    var user = context.User;

                    // بررسی احراز هویت
                    if (user.Identity == null || !user.Identity.IsAuthenticated)
                    {
                        _logger.LogWarning("Unauthorized access attempt to {Path}", context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = "You are not authenticated" });
                        return;
                    }

                    // بررسی نقش‌ها
                    var roles = authorizeAttribute.Roles;
                    if (!string.IsNullOrEmpty(roles))
                    {
                        var requiredRoles = roles.Split(',');
                        var hasRole = requiredRoles.Any(role => user.IsInRole(role.Trim()));

                        if (!hasRole)
                        {
                            _logger.LogWarning("User {UserId} denied access to {Path}. Required roles: {Roles}",
                                user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                                context.Request.Path, roles);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsJsonAsync(new { message = "You don't have permission" });
                            return;
                        }
                    }

                    // بررسی Policy
                    var policy = authorizeAttribute.Policy;
                    if (!string.IsNullOrEmpty(policy))
                    {
                        // در اینجا منطق بررسی Policy را پیاده‌سازی کنید
                        _logger.LogDebug("Policy check for {Policy}", policy);
                    }
                }
            }

            await _next(context);
        }
    }
}
