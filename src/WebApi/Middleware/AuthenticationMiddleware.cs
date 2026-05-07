using Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApi.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _logger;

        public AuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILoggerService logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractToken(context);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var principal = ValidateToken(token);
                    if (principal != null)
                    {
                        context.User = principal;
                        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        _logger.LogDebug("User authenticated: {UserId}, Path: {Path}",
                            userId, context.Request.Path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message, "Token validation failed. Path: {Path}", context.Request.Path);

                    // ادامه می‌دهیم، اما کاربر احراز هویت نشده است
                }
            }

            await _next(context);
        }

        private static string? ExtractToken(HttpContext context)
        {
            // از هدر Authorization
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // از کوکی
            return context.Request.Cookies["accessToken"];
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? Shared.AppConstants.JWTDefaults.Key);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? Shared.AppConstants.JWTDefaults.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? Shared.AppConstants.JWTDefaults.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
