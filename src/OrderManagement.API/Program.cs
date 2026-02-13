using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OrderManagement.API.Middleware;
using OrderManagement.Application.Validators;
using OrderManagement.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ---- Services ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Order Management API",
        Version = "v1",
        Description = "API for managing orders with Azure Service Bus integration"
    });
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Infrastructure (EF, Repos, Services, Messaging)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("Frontend:Url") ?? "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Health Checks
var serviceBusConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"] ?? "";
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: dbConnectionString,
        name: "postgresql",
        tags: new[] { "db", "postgresql" })
    .AddAzureServiceBusQueue(
        connectionString: serviceBusConnectionString,
        queueName: builder.Configuration["AzureServiceBus:QueueName"] ?? "orders",
        name: "azure-service-bus",
        tags: new[] { "messaging", "servicebus" });

var app = builder.Build();

// ---- Middleware Pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Just checks if app is running
});

// Apply migrations automatically
try
{
    Log.Information("Applying database migrations...");
    await DependencyInjection.ApplyMigrationsAsync(app.Services);
    Log.Information("Database migrations applied successfully.");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while applying migrations.");
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Urls;
    foreach (var address in addresses)
    {
        Log.Information("🚀 API is listening on: {Address}", address);
        Log.Information("📖 Swagger UI: {Address}/swagger", address);
        Log.Information("❤️ Health Check: {Address}/health", address);
    }
});

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }