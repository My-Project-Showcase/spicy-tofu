using Application.Automation.Web;
using Domain.Runtime.Environment.Configuration;

using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Infrastructure.Automation.Web;

public sealed class WebDriver : IWebDriver
{
    private const string DefaultSessionName = "default";

    private readonly BrowserHost _browserHost;
    private readonly TestExecution _testExecution;
    private readonly PlaywrightConfig _playwrightConfig;
    private readonly ConcurrentDictionary<string, BrowserSession> _sessions = new();

    public WebDriver(
        BrowserHost browserHost,
        IOptions<TestExecution> testExecution,
        IOptions<PlaywrightConfig> playwrightConfig)
    {
        _browserHost = browserHost;
        _testExecution = testExecution.Value;
        _playwrightConfig = playwrightConfig.Value;
    }

    public IWebPage Page => GetSession(DefaultSessionName).Page;

    public async Task StartAsync()
    {
        await StartSessionAsync(DefaultSessionName);
    }

    public async Task<IWebSession> StartSessionAsync(string name, WebContextOptions? options = null)
    {
        if (_sessions.ContainsKey(name))
        {
            throw new InvalidOperationException($"A session named '{name}' has already been started.");
        }

        BrowserNewContextOptions? contextOptions = options is null
            ? null
            : new BrowserNewContextOptions
            {
                BaseURL = options.BaseUrl,
            };

        var browser = await _browserHost.GetBrowserAsync();
        var context = await browser.NewContextAsync(contextOptions);
        context.SetDefaultTimeout(_testExecution.DefaultTimeoutMs);
        context.SetDefaultNavigationTimeout(_playwrightConfig.NavigationTimeoutMs);

        var page = await context.NewPageAsync();
        var session = new BrowserSession(name, context, page);

        _sessions.TryAdd(name, session);

        return session;
    }

    public IWebSession GetSession(string name)
    {
        if (_sessions.TryGetValue(name, out var session))
        {
            return session;
        }

        throw new InvalidOperationException(
            $"No session named '{name}' is active. Start it with StartSessionAsync before requesting it.");
    }

    public async Task StopAsync()
    {
        foreach (var session in _sessions.Values)
        {
            await session.DisposeAsync();
        }

        _sessions.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}