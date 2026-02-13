using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOrderWithPendingStatus()
    {
        var order = Order.Create("John Doe", "Laptop", 1500.00m);

        order.Id.Should().NotBeEmpty();
        order.CustomerName.Should().Be("John Doe");
        order.ProductName.Should().Be("Laptop");
        order.Value.Should().Be(1500.00m);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        order.UpdatedAt.Should().BeNull();
        order.StatusHistory.Should().HaveCount(1);
        order.StatusHistory.First().Status.Should().Be(OrderStatus.Pending);
    }

    [Theory]
    [InlineData("", "Product", 100)]
    [InlineData("  ", "Product", 100)]
    [InlineData(null, "Product", 100)]
    public void Create_WithInvalidCustomerName_ShouldThrowArgumentException(
        string customerName, string productName, decimal value)
    {
        var act = () => Order.Create(customerName, productName, value);
        act.Should().Throw<ArgumentException>().WithMessage("*Customer name*");
    }

    [Theory]
    [InlineData("Customer", "", 100)]
    [InlineData("Customer", "  ", 100)]
    [InlineData("Customer", null, 100)]
    public void Create_WithInvalidProductName_ShouldThrowArgumentException(
        string customerName, string productName, decimal value)
    {
        var act = () => Order.Create(customerName, productName, value);
        act.Should().Throw<ArgumentException>().WithMessage("*Product name*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_WithInvalidValue_ShouldThrowArgumentException(decimal value)
    {
        var act = () => Order.Create("Customer", "Product", value);
        act.Should().Throw<ArgumentException>().WithMessage("*Value*");
    }

    [Fact]
    public void AdvanceToProcessing_FromPending_ShouldSucceed()
    {
        var order = Order.Create("John", "Phone", 999.99m);

        order.AdvanceToProcessing();

        order.Status.Should().Be(OrderStatus.Processing);
        order.UpdatedAt.Should().NotBeNull();
        order.StatusHistory.Should().HaveCount(2);
        order.StatusHistory.Last().Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void AdvanceToProcessing_FromProcessing_ShouldThrowInvalidOperationException()
    {
        var order = Order.Create("John", "Phone", 999.99m);
        order.AdvanceToProcessing();

        var act = () => order.AdvanceToProcessing();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition to Processing*");
    }

    [Fact]
    public void AdvanceToProcessing_FromCompleted_ShouldThrowInvalidOperationException()
    {
        var order = Order.Create("John", "Phone", 999.99m);
        order.AdvanceToProcessing();
        order.AdvanceToCompleted();

        var act = () => order.AdvanceToProcessing();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdvanceToCompleted_FromProcessing_ShouldSucceed()
    {
        var order = Order.Create("John", "Phone", 999.99m);
        order.AdvanceToProcessing();

        order.AdvanceToCompleted();

        order.Status.Should().Be(OrderStatus.Completed);
        order.UpdatedAt.Should().NotBeNull();
        order.StatusHistory.Should().HaveCount(3);
        order.StatusHistory.Last().Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void AdvanceToCompleted_FromPending_ShouldThrowInvalidOperationException()
    {
        var order = Order.Create("John", "Phone", 999.99m);

        var act = () => order.AdvanceToCompleted();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot transition to Completed*");
    }

    [Fact]
    public void AdvanceToCompleted_FromCompleted_ShouldThrowInvalidOperationException()
    {
        var order = Order.Create("John", "Phone", 999.99m);
        order.AdvanceToProcessing();
        order.AdvanceToCompleted();

        var act = () => order.AdvanceToCompleted();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StatusHistory_ShouldTrackFullLifecycle()
    {
        var order = Order.Create("John", "Phone", 999.99m);
        order.AdvanceToProcessing();
        order.AdvanceToCompleted();

        order.StatusHistory.Should().HaveCount(3);

        var history = order.StatusHistory.OrderBy(h => h.ChangedAt).ToList();
        history[0].Status.Should().Be(OrderStatus.Pending);
        history[1].Status.Should().Be(OrderStatus.Processing);
        history[2].Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void AddOutboxMessage_ShouldAddMessageToCollection()
    {
        var order = Order.Create("John", "Phone", 999.99m);

        order.AddOutboxMessage("OrderCreated", "{\"test\": true}");

        order.OutboxMessages.Should().HaveCount(1);
        order.OutboxMessages.First().EventType.Should().Be("OrderCreated");
        order.OutboxMessages.First().OrderId.Should().Be(order.Id);
        order.OutboxMessages.First().IsProcessed.Should().BeFalse();
    }
}