namespace OrderManagement.Application.Interfaces;

public interface IOrderProcessor
{
    Task ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}