using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Validators;

namespace OrderManagement.UnitTests.Application;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator;

    public CreateOrderRequestValidatorTests()
    {
        _validator = new CreateOrderRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        var request = new CreateOrderRequest("John Doe", "Laptop", 1500.00m);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCustomerName_ShouldHaveError()
    {
        var request = new CreateOrderRequest("", "Laptop", 1500.00m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Validate_WithEmptyProductName_ShouldHaveError()
    {
        var request = new CreateOrderRequest("John", "", 1500.00m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }

    [Fact]
    public void Validate_WithZeroValue_ShouldHaveError()
    {
        var request = new CreateOrderRequest("John", "Laptop", 0);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WithNegativeValue_ShouldHaveError()
    {
        var request = new CreateOrderRequest("John", "Laptop", -50m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_WithTooLongCustomerName_ShouldHaveError()
    {
        var longName = new string('A', 201);
        var request = new CreateOrderRequest(longName, "Laptop", 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Validate_WithTooLongProductName_ShouldHaveError()
    {
        var longName = new string('A', 201);
        var request = new CreateOrderRequest("John", longName, 100m);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ProductName);
    }
}