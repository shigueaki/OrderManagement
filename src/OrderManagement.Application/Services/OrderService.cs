using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mappings;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMessagePublisher messagePublisher,
        ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for customer {CustomerName}, product {ProductName}",
            request.CustomerName, request.ProductName);

        var order = Order.Create(request.CustomerName, request.ProductName, request.Value);

        // Outbox Pattern: persist message in the same transaction as the order
        var message = new OrderCreatedMessage(
            order.Id,
            order.CustomerName,
            order.ProductName,
            order.Value,
            order.CreatedAt
        );

        var payload = JsonSerializer.Serialize(message);
        order.AddOutboxMessage("OrderCreated", payload);

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully with outbox message", order.Id);

        return OrderMapper.ToResponse(order, includeHistory: true);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        return OrderMapper.ToResponseList(orders);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithHistoryAsync(id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found", id);
            return null;
        }

        return OrderMapper.ToResponse(order, includeHistory: true);
    }
}