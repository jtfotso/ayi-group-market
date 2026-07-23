using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace AYIGroupMarket.Infrastructure.Identity;

public static class RoleSeeder
{
    private static readonly string[] Roles = { "Customer", "Wholesale", "Admin" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
    }
}