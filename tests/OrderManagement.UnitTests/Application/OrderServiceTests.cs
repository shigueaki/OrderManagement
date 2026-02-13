using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Application;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IOutboxRepository> _outboxRepoMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _outboxRepoMock = new Mock<IOutboxRepository>();
        _publisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.OutboxMessages).Returns(_outboxRepoMock.Object);

        _sut = new OrderService(_unitOfWorkMock.Object, _publisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ShouldReturnOrderResponse()
    {
        // Arrange
        var request = new CreateOrderRequest("John Doe", "Laptop", 1500.00m);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.CustomerName.Should().Be("John Doe");
        result.ProductName.Should().Be("Laptop");
        result.Value.Should().Be(1500.00m);
        result.Status.Should().Be("Pending");
        result.Id.Should().NotBeEmpty();

        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            Order.Create("John", "Laptop", 1500m),
            Order.Create("Jane", "Phone", 999m)
        };

        _orderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        // Act
        var result = await _sut.GetAllOrdersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithExistingId_ShouldReturnOrder()
    {
        // Arrange
        var order = Order.Create("John", "Laptop", 1500m);
        _orderRepoMock.Setup(r => r.GetByIdWithHistoryAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.GetOrderByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("John");
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetByIdWithHistoryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.GetOrderByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }
}