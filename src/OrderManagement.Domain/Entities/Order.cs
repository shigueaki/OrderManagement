using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property for status history (bonus)
    private readonly List<OrderStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    // For Outbox Pattern (bonus)
    private readonly List<OutboxMessage> _outboxMessages = new();
    public IReadOnlyCollection<OutboxMessage> OutboxMessages => _outboxMessages.AsReadOnly();

    private Order() { } // EF constructor

    public static Order Create(string customerName, string productName, decimal value)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (value <= 0)
            throw new ArgumentException("Value must be greater than zero.", nameof(value));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            ProductName = productName,
            Value = value,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order._statusHistory.Add(OrderStatusHistory.Create(order.Id, OrderStatus.Pending));

        return order;
    }

    public void AdvanceToProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot transition to Processing from {Status}. Order must be Pending.");

        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        _statusHistory.Add(OrderStatusHistory.Create(Id, OrderStatus.Processing));
    }

    public void AdvanceToCompleted()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException(
                $"Cannot transition to Completed from {Status}. Order must be Processing.");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        _statusHistory.Add(OrderStatusHistory.Create(Id, OrderStatus.Completed));
    }

    public void AddOutboxMessage(string eventType, string payload)
    {
        _outboxMessages.Add(OutboxMessage.Create(Id, eventType, payload));
    }
}