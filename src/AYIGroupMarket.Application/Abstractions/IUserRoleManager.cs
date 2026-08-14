namespace AYIGroupMarket.Application.Abstractions;

public interface IUserRoleManager
{
    Task AddToRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
    Task RemoveFromRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}