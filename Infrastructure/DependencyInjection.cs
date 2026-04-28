using Domain.Entities;
using Domain.Interfaces;
using Domain.Settings;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;   
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ==========================================
            // 1. دیتابیس
            // ==========================================
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // ==========================================
            // 2. ASP.NET Core Identity
            // ==========================================
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // تنظیمات رمز عبور
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // تنظیمات قفل شدن حساب
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // تنظیمات کاربر
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // تنظیمات ورود
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<ApplicationRole>();

            // ==========================================
            // 3. JWT Authentication
            // ==========================================
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "your-super-secret-key-here-at-least-32-chars-long"))
                };
            });

            // تنظیمات Authorization (اختیاری)
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ActiveUser", policy => policy.RequireClaim("IsActive", "true"));
            });
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));


            // ==========================================
            // 4. Redis Cache (ادغام با سیستم کش)
            // ==========================================
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = configuration.GetConnectionString("Redis");
            //    options.InstanceName = "NotesApi_";
            //});
            //services.AddScoped<ICacheService, RedisCacheService>();

            // ==========================================
            // 5. RabbitMQ
            // ==========================================
            //services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
            //services.AddSingleton<IMessageBus, RabbitMqMessageBus>();


            // ==========================================
            // 6. سرویس JWT
            // ==========================================
            services.AddScoped<IAuthService, JwtTokenService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IDapperService, DapperService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            //services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
