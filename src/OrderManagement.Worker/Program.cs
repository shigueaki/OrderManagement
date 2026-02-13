using OrderManagement.Infrastructure;
using OrderManagement.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// ---- Serilog ----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Infrastructure (EF, Repos, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Worker
builder.Services.AddHostedService<OrderConsumerWorker>();

var host = builder.Build();

// Apply migrations
try
{
    Log.Information("Worker: Applying database migrations...");
    await DependencyInjection.ApplyMigrationsAsync(host.Services);
    Log.Information("Worker: Database migrations applied successfully.");
}
catch (Exception ex)
{
    Log.Error(ex, "Worker: Error applying migrations.");
}

Log.Information("Order Consumer Worker starting...");
host.Run();