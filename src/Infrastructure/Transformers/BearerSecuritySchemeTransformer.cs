using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Infrastructure.Transformers
{
    internal sealed class BearerSecuritySchemeTransformer(
     IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                // 1. ایجاد Security Scheme
                var bearerScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                };

                // 2. افزودن به Components سند OpenAPI
                document.Components ??= new OpenApiComponents();
                document.AddComponent("Bearer", bearerScheme);

                // 3. ایجاد Security Requirement با استفاده از API جدید دات‌نت 10
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                };

                // 4. اعمال به تمام Endpointها (به صورت سراسری)
                if (document.Paths is null) return;

                foreach (var path in document.Paths.Values)
                {
                    if (path?.Operations is null) continue;

                    // تبدیل به لیست برای جلوگیری از خطای تغییر در حین iteration
                    var operationKeys = path.Operations.Keys.ToList();

                    foreach (var operationType in operationKeys)
                    {
                        var operation = path.Operations[operationType];
                        operation.Security ??= [];
                        operation.Security.Add(securityRequirement);
                    }
                }
            }
        }
    }
}