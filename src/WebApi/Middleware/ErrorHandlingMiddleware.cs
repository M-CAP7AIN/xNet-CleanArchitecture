using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi.Middleware
{
    public class ErrorResponse
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public int? StatusCode { get; set; }
        public string? TraceId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Detail { get; set; }
        public string? ExceptionType { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    errorResponse.Title = "Unauthorized";
                    errorResponse.Message = exception.Message ?? "You are not authorized to access this resource";
                    break;

                case KeyNotFoundException:
                case FileNotFoundException:
                    response.StatusCode = StatusCodes.Status404NotFound;
                    errorResponse.Title = "Not Found";
                    errorResponse.Message = exception.Message ?? "The requested resource was not found";
                    break;

                case ArgumentException:
                case InvalidOperationException:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse.Title = "Bad Request";
                    errorResponse.Message = exception.Message;
                    break;

                case FluentValidation.ValidationException validationEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse.Title = "Validation Failed";
                    errorResponse.Message = "One or more validation errors occurred";
                    errorResponse.Errors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    break;

                default:
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse.Title = "Internal Server Error";
                    errorResponse.Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "An error occurred while processing your request";
                    break;
            }

            if (_environment.IsDevelopment())
            {
                errorResponse.Detail = exception.StackTrace;
                errorResponse.ExceptionType = exception.GetType().Name;
            }

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            await response.WriteAsync(json);
        }
    }
}
