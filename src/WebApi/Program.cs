using Application;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.SeedData;
using Infrastructure.Transformers;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NotesApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/notes-api-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
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
    await connectionManager.GetConnectionAsync();
}

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
