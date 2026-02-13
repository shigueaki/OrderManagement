using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Worker;

public class OrderConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderConsumerWorker> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;

    public OrderConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var connectionString = configuration["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("AzureServiceBus:ConnectionString is not configured.");

        var queueName = configuration["AzureServiceBus:QueueName"]
            ?? throw new InvalidOperationException("AzureServiceBus:QueueName is not configured.");

        _client = new ServiceBusClient(connectionString);
        _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            PrefetchCount = 0
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Consumer Worker starting...");

        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("Order Consumer Worker started. Listening for messages...");

        // Keep the worker alive until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Order Consumer Worker stopping...");
        }
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        var messageBody = args.Message.Body.ToString();
        var correlationId = args.Message.CorrelationId;
        var eventType = args.Message.ApplicationProperties.TryGetValue("EventType", out var et)
            ? et.ToString()
            : "Unknown";

        _logger.LogInformation(
            "Received message. CorrelationId: {CorrelationId}, EventType: {EventType}, MessageId: {MessageId}",
            correlationId, eventType, args.Message.MessageId);

        if (eventType != "OrderCreated")
        {
            _logger.LogWarning("Unknown event type: {EventType}. Completing message.", eventType);
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        try
        {
            var orderMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(messageBody);

            if (orderMessage is null)
            {
                _logger.LogError("Failed to deserialize message body. Completing message to avoid poison queue.");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            _logger.LogInformation("Processing order {OrderId} from message", orderMessage.OrderId);

            using var scope = _scopeFactory.CreateScope();
            var orderProcessor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();

            await orderProcessor.ProcessOrderAsync(orderMessage.OrderId, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation("Message for Order {OrderId} completed successfully.", orderMessage.OrderId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error. Dead-lettering message.");
            await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation. Completing message (idempotent).");
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message. Will be retried by Service Bus.");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error. Source: {ErrorSource}, Namespace: {Namespace}, EntityPath: {EntityPath}",
            args.ErrorSource, args.FullyQualifiedNamespace, args.EntityPath);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order Consumer Worker shutting down...");

        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await _client.DisposeAsync();

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("Order Consumer Worker shut down.");
    }
}