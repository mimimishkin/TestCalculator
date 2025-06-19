using TestCalculator.Domain;

namespace TestCalculator.WebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication
            .CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddScoped<ICalculator, Calculator>();

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }
}