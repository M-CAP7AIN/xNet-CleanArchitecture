using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Transformers
{
    public static class ScalarOptions
    {
        public static void ConfigureScalar(this WebApplication app)
        {
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Notes API Documentation");
                options.WithTheme(ScalarTheme.Purple);
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

                // اضافه کردن دو نسخه
                options.AddDocument("v1", "Version 1.0 (Legacy)", "/openapi/v1.json", isDefault: true);
                options.AddDocument("v2", "Version 2.0 (Latest)", "/openapi/v2.json");


                // تنها خط مهم برای JWT - معرفی طرح احراز هویت
                options.Authentication = new ScalarAuthenticationOptions
                {
                    PreferredSecuritySchemes = ["Bearer"]
                };
            });
        }
    }
}
