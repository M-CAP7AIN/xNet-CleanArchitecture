using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Transformers
{
    public class VersionInfoTransformer : IOpenApiDocumentTransformer
    {
        private readonly string _title;
        private readonly string _version;
        private readonly string _description;

        public VersionInfoTransformer(string title, string version, string description)
        {
            _title = title;
            _version = version;
            _description = description;
        }

        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            document.Info = new OpenApiInfo
            {
                Title = _title,
                Version = _version,
                Description = _description,
                Contact = new OpenApiContact
                {
                    Name = "Support Team",
                    Email = "support@example.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            };

            return Task.CompletedTask;
        }
    }
}
