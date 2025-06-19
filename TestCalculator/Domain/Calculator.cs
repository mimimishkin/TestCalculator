namespace TestCalculator.Domain;

public class Calculator : ICalculator
{
    public double Add(double x, double y)
    {
        return x + y;
    }

    public double Subtract(double x, double y)
    {
        return x - y;
    }

    public double Multiply(double x, double y)
    {
        return x * y;
    }

    public double Divide(double x, double y)
    {
        if (y == 0)
            throw new DivideByZeroException();
        return x / y;
    }

    public double Power(double baseNumber, double exponent)
    {
        return Math.Pow(baseNumber, exponent);
    }

    public double Root(double number, double nthRoot)
    {
        if (nthRoot == 0)
            throw new ArgumentException("Root cannot be zero", nameof(nthRoot));
        
        if (number < 0 && nthRoot % 2 == 0)
            throw new ArgumentException("Number cannot be lower than zero for even root", nameof(nthRoot));
        
        return Math.Pow(number, 1.0 / nthRoot);
    }
}
