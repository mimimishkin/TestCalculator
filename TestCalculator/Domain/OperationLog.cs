namespace TestCalculator.Domain;

public class OperationLog
{
    public int Id { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public string? Result { get; set; }
    public DateTime Timestamp { get; set; }
} 