using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AYIGroupMarket.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("../AYIGroupMarket.Web/appsettings.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=AYIGroupMarket;User ID=sa;Password=Blessingsofgod12!;TrustServerCertificate=True;MultipleActiveResultSets=true";

        builder.UseSqlServer(connectionString);
        return new AppDbContext(builder.Options);
    }
}