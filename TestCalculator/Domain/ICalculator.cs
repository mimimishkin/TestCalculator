namespace TestCalculator.Domain;

public interface ICalculator
{
    double Add(double x, double y);

    double Subtract(double x, double y);

    double Multiply(double x, double y);

    double Divide(double x, double y);

    double Power(double baseNumber, double exponent);

    double Root(double number, double nthRoot);
}
