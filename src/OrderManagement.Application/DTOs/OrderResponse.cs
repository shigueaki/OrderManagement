using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public record OrderResponse(
    Guid Id,
    string CustomerName,
    string ProductName,
    decimal Value,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderStatusHistoryResponse>? StatusHistory
);

public record OrderStatusHistoryResponse(
    Guid Id,
    string Status,
    DateTime ChangedAt
);