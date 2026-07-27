namespace AYIGroupMarket.Application.Abstractions;

public interface IWholesaleRoleAssigner
{
    Task AddToWholesaleRoleAsync(string userId, CancellationToken cancellationToken = default);
    Task RemoveFromWholesaleRoleAsync(string userId, CancellationToken cancellationToken = default);
}