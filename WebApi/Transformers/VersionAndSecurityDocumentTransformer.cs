using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Transformers
{
    public sealed class VersionAndSecurityDocumentTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            // تنظیم اطلاعات نسخه
            document.Info = new OpenApiInfo
            {
                Title = "Notes API",
                Version = "v1.0.0",
                Description = "A REST API for managing notes with JWT authentication.",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com"
                }
            };

            return Task.CompletedTask;
        }
    }
}
