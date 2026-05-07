using Application;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.SeedData;
using Infrastructure.Transformers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using Serilog;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NotesApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();
builder.Host.UseSerilog();

// Add OpenApi (JWT & Bearer & Scalar UI & Versioning)
builder.Services.AddOpenApiWithVersions();

// Add Infrastructure (DbContext & SQL Server)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Layer (MediatR, AutoMapper, Validators, Behaviors)
builder.Services.AddApplication();

// Add custom services
//builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Configure CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Seed
    await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);

    // RabbitMq
    var connectionManager = scope.ServiceProvider.GetRequiredService<IRabbitMqConnectionManager>();
    //await connectionManager.GetConnectionAsync();
}

// Middlewares
//app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.ConfigureScalar();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
