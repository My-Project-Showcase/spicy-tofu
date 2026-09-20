namespace Infrastructure.Automation.Mobile.Devices;

public interface IDeviceLauncher : IAsyncDisposable
{
    Task EnsureStartedAsync();

    Task ShutdownIfStartedAsync();
}
