using Admin.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure all services
builder.ConfigureServices();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
await app.ConfigurePipeline();

// Start the application
await app.RunAsync();
