namespace OrderManagement.Application.DTOs;

public record CreateOrderRequest(
    string CustomerName,
    string ProductName,
    decimal Value
);