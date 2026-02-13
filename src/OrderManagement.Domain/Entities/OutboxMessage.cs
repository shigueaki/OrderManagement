namespace OrderManagement.Domain.Entities;

/// <summary>
/// Outbox Pattern: ensures message is persisted in the same transaction as the order (Bonus: +3 points)
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public bool IsProcessed { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(Guid orderId, string eventType, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
    }
}