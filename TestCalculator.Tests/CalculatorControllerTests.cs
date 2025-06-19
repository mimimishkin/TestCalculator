using Microsoft.AspNetCore.Mvc;
using TestCalculator.WebApi;
using TestCalculator.Domain;
using Microsoft.EntityFrameworkCore;

namespace TestCalculator.Tests;

public class CalculatorControllerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly CalculatorController _controller;

    public CalculatorControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;
        _dbContext = new AppDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
        _controller = new CalculatorController(new Calculator(), _dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public void Add_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Add(2, 3) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(5d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Subtract_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Subtract(5, 2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(3d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Multiply_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Multiply(4, 2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(8d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Divide_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Divide(10, 2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(5d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Divide_ByZero_ReturnsBadRequest()
    {
        var result = _controller.Divide(10, 0) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("divide by zero", result.Value!.ToString());
    }
    
    [Fact]
    public void Power_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Power(2, 3) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(8d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Root_ReturnsOkResult_WithExpectedValue()
    {
        var result = _controller.Root(16, 2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(4d, (double)result.Value!.GetType().GetProperty("result")!.GetValue(result.Value)!);
    }

    [Fact]
    public void Root_WithNegativeNumber_ReturnsBadRequest()
    {
        var result = _controller.Root(-16, 2) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Number cannot be lower than zero for even root", result.Value!.ToString());
    }

    [Fact]
    public void Add_LogsOperationToDatabase()
    {
        _controller.Add(2, 3);
        var log = _dbContext.OperationLogs.FirstOrDefault(l => l.Operation == "Add");
        Assert.NotNull(log);
        Assert.Contains("\"a\":2", log.Parameters);
        Assert.Contains("\"b\":3", log.Parameters);
        Assert.Equal("5", log.Result);
    }
}

