using Microsoft.AspNetCore.Mvc;
using TestCalculator.Domain;
using Microsoft.AspNetCore.Authorization;

namespace TestCalculator.WebApi;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalculatorController(ICalculator calculator, AppDbContext dbContext) : ControllerBase
{
    private IActionResult HandleCalculation(string operation, object parameters, Func<double> calculation)
    {
        try
        {
            var result = calculation();
            dbContext.OperationLogs.Add(new OperationLog
            {
                Operation = operation,
                Parameters = System.Text.Json.JsonSerializer.Serialize(parameters),
                Result = result.ToString(),
                Timestamp = DateTime.UtcNow
            });
            dbContext.SaveChanges();
            return Ok(new { result });
        }
        catch (Exception ex)
        {
            dbContext.OperationLogs.Add(new OperationLog
            {
                Operation = operation,
                Parameters = System.Text.Json.JsonSerializer.Serialize(parameters),
                Result = $"Error: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
            dbContext.SaveChanges();
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("add")]
    public IActionResult Add([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation("Add", new { a, b }, () => calculator.Add(a, b));
    }

    [HttpGet("subtract")]
    public IActionResult Subtract([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation("Subtract", new { a, b }, () => calculator.Subtract(a, b));
    }

    [HttpGet("multiply")]
    public IActionResult Multiply([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation("Multiply", new { a, b }, () => calculator.Multiply(a, b));
    }

    [HttpGet("divide")]
    public IActionResult Divide([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation("Divide", new { a, b }, () => calculator.Divide(a, b));
    }

    [HttpGet("power")]
    public IActionResult Power([FromQuery] double baseNumber, [FromQuery] double exponent)
    {
        return HandleCalculation("Power", new { baseNumber, exponent }, () => calculator.Power(baseNumber, exponent));
    }

    [HttpGet("root")]
    public IActionResult Root([FromQuery] double number, [FromQuery] double nthRoot)
    {
        return HandleCalculation("Root", new { number, nthRoot }, () => calculator.Root(number, nthRoot));
    }
}