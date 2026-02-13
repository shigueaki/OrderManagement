using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Infrastructure.Messaging;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
            }));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderProcessor, OrderProcessor>();

        // Messaging - check if Service Bus is configured
        var serviceBusConnectionString = configuration["AzureServiceBus:ConnectionString"];
        var isServiceBusConfigured = !string.IsNullOrWhiteSpace(serviceBusConnectionString)
            && serviceBusConnectionString != "YOUR_SERVICE_BUS_CONNECTION_STRING_HERE";

        if (isServiceBusConfigured)
        {
            // Real Azure Service Bus publisher
            services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();

            // Outbox Processor (background service) - only when Service Bus is available
            services.AddHostedService<OutboxProcessor>();
        }
        else
        {
            // Fallback: in-memory publisher (logs messages locally)
            services.AddSingleton<IMessagePublisher, InMemoryMessagePublisher>();
        }

        return services;
    }

    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply migrations. Attempting to create database...");

            // Try to ensure database exists at minimum
            try
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created using EnsureCreated.");
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Failed to create database.");
                throw;
            }
        }
    }
}