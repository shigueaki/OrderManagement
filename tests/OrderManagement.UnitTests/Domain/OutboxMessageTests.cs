using FluentAssertions;
using OrderManagement.Domain.Entities;

namespace OrderManagement.UnitTests.Domain;

public class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldInitializeCorrectly()
    {
        var orderId = Guid.NewGuid();

        var message = OutboxMessage.Create(orderId, "OrderCreated", "{\"data\": 1}");

        message.Id.Should().NotBeEmpty();
        message.OrderId.Should().Be(orderId);
        message.EventType.Should().Be("OrderCreated");
        message.Payload.Should().Be("{\"data\": 1}");
        message.IsProcessed.Should().BeFalse();
        message.ProcessedAt.Should().BeNull();
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsProcessed_ShouldUpdateFlags()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), "OrderCreated", "{}");

        message.MarkAsProcessed();

        message.IsProcessed.Should().BeTrue();
        message.ProcessedAt.Should().NotBeNull();
        message.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}