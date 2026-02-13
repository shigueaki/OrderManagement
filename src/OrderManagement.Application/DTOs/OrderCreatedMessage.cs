namespace OrderManagement.Application.DTOs;

public record OrderCreatedMessage(
    Guid OrderId,
    string CustomerName,
    string ProductName,
    decimal Value,
    DateTime CreatedAt
);