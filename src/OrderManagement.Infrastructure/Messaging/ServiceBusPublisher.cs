using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Messaging;

public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("AzureServiceBus:ConnectionString is not configured.");

        var queueName = configuration["AzureServiceBus:QueueName"]
            ?? throw new InvalidOperationException("AzureServiceBus:QueueName is not configured.");

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task PublishOrderCreatedAsync(Guid orderId, string payload, CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(payload)
        {
            ContentType = "application/json",
            CorrelationId = orderId.ToString(),
            Subject = "OrderCreated",
            MessageId = Guid.NewGuid().ToString(),
            ApplicationProperties =
            {
                { "EventType", "OrderCreated" },
                { "OrderId", orderId.ToString() }
            }
        };

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("Published OrderCreated message for Order {OrderId} to Service Bus", orderId);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}   