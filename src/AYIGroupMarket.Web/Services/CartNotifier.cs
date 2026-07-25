namespace AYIGroupMarket.Web.Services;

public class CartNotifier
{
    public event Func<Task>? OnCartChanged;

    public async Task NotifyCartChangedAsync()
    {
        if (OnCartChanged is not null)
            await OnCartChanged.Invoke();
    }
}