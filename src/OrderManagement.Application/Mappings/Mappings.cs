using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Mappings;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order, bool includeHistory = false)
    {
        List<OrderStatusHistoryResponse>? history = null;

        if (includeHistory && order.StatusHistory.Any())
        {
            history = order.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryResponse(
                    h.Id,
                    h.Status.ToString(),
                    h.ChangedAt
                ))
                .ToList();
        }

        return new OrderResponse(
            order.Id,
            order.CustomerName,
            order.ProductName,
            order.Value,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            history
        );
    }

    public static IEnumerable<OrderResponse> ToResponseList(IEnumerable<Order> orders)
    {
        return orders.Select(o => ToResponse(o, false));
    }
}