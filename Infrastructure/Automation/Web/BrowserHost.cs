using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Infrastructure.Automation.Web;

public sealed class BrowserHost : IAsyncDisposable
{
    private readonly IOptions<PlaywrightConfig> _playwrightConfig;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private IPlaywright? _playwright;
    private volatile IBrowser? _browser;
    
    public BrowserHost(IOptions<PlaywrightConfig> playwrightConfig)
    {
        _playwrightConfig = playwrightConfig;
    }

    public async Task<IBrowser> GetBrowserAsync()
    {
        var existing = _browser;
        if (existing is not null)
        {
            return existing;
        }

        await _sync.WaitAsync();
        try
        {
            if (_browser is not null)
            {
                return _browser;
            }

            _playwright = await Playwright.CreateAsync();

            try
            {
                _browser = await GetBrowserType(_playwright)
                    .LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = _playwrightConfig.Value.Headless,
                    });
            }
            catch
            {
                _playwright.Dispose();
                _playwright = null;
                throw;
            }

            return _browser;
        }
        finally
        {
            _sync.Release();
        }
    }

    private IBrowserType GetBrowserType(IPlaywright playwright)
    {
        return _playwrightConfig.Value.Browser.ToLowerInvariant() switch
        {
            "firefox" => playwright.Firefox,
            "webkit" => playwright.Webkit,
            _ => playwright.Chromium,
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;

        _sync.Dispose();
    }
}