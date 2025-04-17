using Admin.Infrastructure.Persistence;
using Admin.WebAPI.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


// Configure all services
builder.ConfigureServices();

// Build the application
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
await app.ConfigurePipeline();

// Start the application
await app.RunAsync();
