using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Application;

public class OrderProcessorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ILogger<OrderProcessor>> _loggerMock;
    private readonly OrderProcessor _sut;

    public OrderProcessorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<OrderProcessor>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);

        _sut = new OrderProcessor(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithPendingOrder_ShouldAdvanceToCompleted()
    {
        var order = Order.Create("John", "Laptop", 1500m);

        _orderRepoMock.SetupSequence(r => r.GetByIdWithHistoryAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order)
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.ProcessOrderAsync(order.Id);

        order.Status.Should().Be(OrderStatus.Completed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessOrderAsync_WithNonExistingOrder_ShouldNotThrow()
    {
        var id = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetByIdWithHistoryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _sut.ProcessOrderAsync(id);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessOrderAsync_WithAlreadyCompletedOrder_ShouldBeIdempotent()
    {
        var order = Order.Create("John", "Laptop", 1500m);
        order.AdvanceToProcessing();
        order.AdvanceToCompleted();

        _orderRepoMock.Setup(r => r.GetByIdWithHistoryAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _sut.ProcessOrderAsync(order.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_WithAlreadyProcessingOrder_ShouldBeIdempotent()
    {
        var order = Order.Create("John", "Laptop", 1500m);
        order.AdvanceToProcessing();

        _orderRepoMock.Setup(r => r.GetByIdWithHistoryAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _sut.ProcessOrderAsync(order.Id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}