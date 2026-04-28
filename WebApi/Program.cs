using Application;
using Infrastructure;
using Infrastructure.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using WebApi.Transformers;
using Infrastructure.Transformers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


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

// Seed
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
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
