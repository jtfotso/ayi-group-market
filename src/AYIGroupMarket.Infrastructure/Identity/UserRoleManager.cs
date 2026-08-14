using Microsoft.AspNetCore.Identity;
using AYIGroupMarket.Application.Abstractions;

namespace AYIGroupMarket.Infrastructure.Identity;

public class UserRoleManager(UserManager<ApplicationUser> userManager) : IUserRoleManager
{
    public async Task AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        if (!await userManager.IsInRoleAsync(user, roleName))
            await userManager.AddToRoleAsync(user, roleName);
    }

    public async Task RemoveFromRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        if (await userManager.IsInRoleAsync(user, roleName))
            await userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        await userManager.DeleteAsync(user);
    }
}