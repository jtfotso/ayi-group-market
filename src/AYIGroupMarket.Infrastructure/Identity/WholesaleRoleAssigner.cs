using Microsoft.AspNetCore.Identity;
using AYIGroupMarket.Application.Abstractions;

namespace AYIGroupMarket.Infrastructure.Identity;

public class WholesaleRoleAssigner(UserManager<ApplicationUser> userManager) : IWholesaleRoleAssigner
{
    private const string RoleName = "Wholesale";

    public async Task AddToWholesaleRoleAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        if (!await userManager.IsInRoleAsync(user, RoleName))
            await userManager.AddToRoleAsync(user, RoleName);
    }

    public async Task RemoveFromWholesaleRoleAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        if (await userManager.IsInRoleAsync(user, RoleName))
            await userManager.RemoveFromRoleAsync(user, RoleName);
    }
}