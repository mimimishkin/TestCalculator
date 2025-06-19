using Microsoft.AspNetCore.Mvc;
using TestCalculator.Domain;

namespace TestCalculator.WebApi;

[ApiController]
[Route("api/[controller]")]
public class CalculatorController(ICalculator calculator) : ControllerBase
{
    private IActionResult HandleCalculation(Func<double> calculation)
    {
        try
        {
            var result = calculation();
            return Ok(new { result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("add")]
    public IActionResult Add([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation(() => calculator.Add(a, b));
    }

    [HttpGet("subtract")]
    public IActionResult Subtract([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation(() => calculator.Subtract(a, b));
    }

    [HttpGet("multiply")]
    public IActionResult Multiply([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation(() => calculator.Multiply(a, b));
    }

    [HttpGet("divide")]
    public IActionResult Divide([FromQuery] double a, [FromQuery] double b)
    {
        return HandleCalculation(() => calculator.Divide(a, b));
    }
}