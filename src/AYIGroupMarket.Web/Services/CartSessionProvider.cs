using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AYIGroupMarket.Web.Services;

public class CartSessionProvider(
    ProtectedLocalStorage protectedLocalStorage,
    AuthenticationStateProvider authStateProvider)
{
    private const string StorageKey = "ayi_cart_session_id";
    private string? _cachedOwnerKey;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GetOwnerKeyAsync()
    {
        if (_cachedOwnerKey is not null)
            return _cachedOwnerKey;

        await _lock.WaitAsync();
        try
        {
            // Re-check after acquiring the lock — another caller may have just finished
            if (_cachedOwnerKey is not null)
                return _cachedOwnerKey;

            var authState = await authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _cachedOwnerKey = $"user:{userId}";
                return _cachedOwnerKey;
            }

            try
            {
                var existing = await protectedLocalStorage.GetAsync<Guid>(StorageKey);
                if (existing.Success)
                {
                    _cachedOwnerKey = $"session:{existing.Value}";
                    return _cachedOwnerKey;
                }
            }
            catch (InvalidOperationException)
            {
                // JS interop not yet available (prerendering) — fall through
            }

            var newSessionId = Guid.NewGuid();
            try
            {
                await protectedLocalStorage.SetAsync(StorageKey, newSessionId);
            }
            catch (InvalidOperationException)
            {
                // Still prerendering — this attempt's key won't persist; next real call will retry
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Stored value was encrypted with a key that's no longer in the keyring
                // (e.g. stale data from before persistent keys were configured) — treat as absent
            }
            _cachedOwnerKey = $"session:{newSessionId}";
            return _cachedOwnerKey;
        }
        finally
        {
            _lock.Release();
        }
    }
}