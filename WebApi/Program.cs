using Infrastructure;
using Scalar.AspNetCore;
using Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Add Infrastructure (DbContext & SQL Server)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Layer (MediatR, AutoMapper, Validators, Behaviors)
builder.Services.AddApplication();

// Add custom services
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
