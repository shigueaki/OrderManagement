using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrderManagement.Application.DTOs;

namespace OrderManagement.IntegrationTests;

public class OrdersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = new CreateOrderRequest("John Doe", "Laptop Pro", 2500.00m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        order.Should().NotBeNull();
        order!.CustomerName.Should().Be("John Doe");
        order.ProductName.Should().Be("Laptop Pro");
        order.Value.Should().Be(2500.00m);
        order.Status.Should().Be("Pending");
        order.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateOrder_WithEmptyCustomerName_ShouldReturn400()
    {
        var request = new CreateOrderRequest("", "Laptop", 1500.00m);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithZeroValue_ShouldReturn400()
    {
        var request = new CreateOrderRequest("John", "Laptop", 0);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithNegativeValue_ShouldReturn400()
    {
        var request = new CreateOrderRequest("John", "Laptop", -100m);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturn200WithList()
    {
        // Arrange - create some orders
        await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Alice", "Phone", 999m));
        await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Bob", "Tablet", 599m));

        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>(_jsonOptions);
        orders.Should().NotBeNull();
        orders!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetOrderById_WithExistingId_ShouldReturn200()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Charlie", "Monitor", 450m));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/orders/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(created.Id);
        order.CustomerName.Should().Be("Charlie");
        order.StatusHistory.Should().NotBeNull();
        order.StatusHistory!.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistingId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_ShouldHaveStatusHistory()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Diana", "Keyboard", 150m));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/orders/{created!.Id}");
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(_jsonOptions);

        // Assert
        order!.StatusHistory.Should().NotBeNull();
        order.StatusHistory!.Should().HaveCount(1);
        order.StatusHistory.First().Status.Should().Be("Pending");
    }
}