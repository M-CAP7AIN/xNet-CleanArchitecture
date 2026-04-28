using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Transformers
{
    public static class OpenApiRegistration
    {
        public static IServiceCollection AddOpenApiWithVersions(this IServiceCollection services)
        {
            // نسخه 1
            services.AddOpenApi("v1", options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddDocumentTransformer(new VersionInfoTransformer(
                    "Notes API",
                    "1.0.0",
                    "Legacy version of Notes API"
                ));
            });

            // نسخه 2
            services.AddOpenApi("v2", options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddDocumentTransformer(new VersionInfoTransformer(
                    "Notes API",
                    "2.0.0",
                    "Latest version with enhanced features"
                ));
            });

            return services;
        }
    }
}
