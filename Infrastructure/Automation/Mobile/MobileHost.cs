using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile.Devices;

using Microsoft.Extensions.Options;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileHost : IAsyncDisposable
{
    private readonly AppiumServerLauncher _serverLauncher;
    private readonly IDeviceLauncher _deviceLauncher;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private bool _ready;

    public MobileHost(IOptions<AppiumConfig> appiumOptions)
    {
        var appium = appiumOptions.Value;
        _serverLauncher = new AppiumServerLauncher(appium);
        _deviceLauncher = string.Equals(
            appium.PlatformName,
            "iOS",
            StringComparison.OrdinalIgnoreCase)
            ? new IosSimulatorLauncher(appium)
            : new AndroidEmulatorLauncher(appium);
    }

    public async Task EnsureEnvironmentReadyAsync()
    {
        if (_ready)
        {
            return;
        }

        await _sync.WaitAsync();
        try
        {
            if (_ready)
            {
                return;
            }

            await _serverLauncher.EnsureStartedAsync();
            await _deviceLauncher.EnsureStartedAsync();
            _ready = true;
        }
        finally
        {
            _sync.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _deviceLauncher.DisposeAsync();
        await _serverLauncher.DisposeAsync();
        _sync.Dispose();
    }
}
