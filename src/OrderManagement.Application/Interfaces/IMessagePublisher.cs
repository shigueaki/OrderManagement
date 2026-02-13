namespace OrderManagement.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishOrderCreatedAsync(Guid orderId, string payload, CancellationToken cancellationToken = default);
}