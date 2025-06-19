using TestCalculator.Domain;
using Microsoft.EntityFrameworkCore;

namespace TestCalculator.WebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication
            .CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddScoped<ICalculator, Calculator>();
        builder.Services.AddDbContext<OperationLogDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        app.MapControllers();

        // Ensure a database is created and migrated
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OperationLogDbContext>();
            db.Database.EnsureCreated();
        }

        app.Run();
    }
}