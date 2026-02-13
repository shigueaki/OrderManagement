using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class OrderProcessor : IOrderProcessor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(IUnitOfWork unitOfWork, ILogger<OrderProcessor> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order {OrderId}", orderId);

        var order = await _unitOfWork.Orders.GetByIdWithHistoryAsync(orderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found. Skipping.", orderId);
            return;
        }

        // Idempotency: if already processing or completed, skip
        if (order.Status == OrderStatus.Processing || order.Status == OrderStatus.Completed)
        {
            _logger.LogInformation("Order {OrderId} is already {Status}. Skipping (idempotent).",
                orderId, order.Status);
            return;
        }

        // Step 1: Advance to Processing
        order.AdvanceToProcessing();
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderId} status updated to Processing", orderId);

        // Step 2: Simulate processing delay (5 seconds)
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        // Step 3: Advance to Completed
        // Re-fetch to ensure fresh state (idempotency)
        order = await _unitOfWork.Orders.GetByIdWithHistoryAsync(orderId, cancellationToken);

        if (order is null || order.Status == OrderStatus.Completed)
        {
            _logger.LogInformation("Order {OrderId} already completed or not found. Skipping.", orderId);
            return;
        }

        order.AdvanceToCompleted();
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to Completed", orderId);
    }
}