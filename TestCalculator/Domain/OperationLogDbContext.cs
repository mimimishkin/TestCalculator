using Microsoft.EntityFrameworkCore;

namespace TestCalculator.Domain;

public class OperationLogDbContext(DbContextOptions<OperationLogDbContext> options) : DbContext(options)
{
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();
    public DbSet<User> Users => Set<User>();
} 