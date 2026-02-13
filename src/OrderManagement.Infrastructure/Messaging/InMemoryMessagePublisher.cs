using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Messaging;

/// <summary>
/// Fallback publisher when Azure Service Bus is not configured.
/// Logs the message instead of sending it to a real queue.
/// </summary>
public class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly ILogger<InMemoryMessagePublisher> _logger;

    public InMemoryMessagePublisher(ILogger<InMemoryMessagePublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishOrderCreatedAsync(Guid orderId, string payload, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[IN-MEMORY] Azure Service Bus not configured. Message for Order {OrderId} logged locally. Payload: {Payload}",
            orderId, payload);

        return Task.CompletedTask;
    }
}