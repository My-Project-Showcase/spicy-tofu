using System.Collections.Concurrent;

using Application.Automation.Mobile;
using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.Options;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileDriver : IMobileDriver
{
    private const string DefaultSessionName = "default";

    private readonly MobileHost _mobileHost;
    private readonly TestExecution _testExecution;
    private readonly AppiumConfig _appium;
    private readonly ConcurrentDictionary<string, MobileSession> _sessions = new();

    public MobileDriver(
        MobileHost mobileHost,
        IOptions<TestExecution> testExecution,
        IOptions<AppiumConfig> appium)
    {
        _mobileHost = mobileHost;
        _testExecution = testExecution.Value;
        _appium = appium.Value;
    }

    public async Task StartAsync()
    {
        await StartSessionAsync(DefaultSessionName);
    }

    public async Task<IMobileSession> StartSessionAsync(string name, MobileContextOptions? options = null)
    {
        if (_sessions.ContainsKey(name))
        {
            throw new InvalidOperationException($"A session named '{name}' has already been started.");
        }

        await _mobileHost.EnsureEnvironmentReadyAsync();

        var driver = CreateDriver(_appium, _testExecution.DefaultTimeoutMs);
        var session = new MobileSession(name, driver);

        _sessions.TryAdd(name, session);

        return session;
    }

    public IMobileSession GetSession(string name)
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
            session.Driver.Dispose();
        }

        _sessions.Clear();

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private static AppiumDriver CreateDriver(AppiumConfig appium, int defaultTimeoutMs)
    {
        var options = new AppiumOptions
        {
            AutomationName = appium.AutomationName,
            PlatformName = appium.PlatformName,
            DeviceName = appium.DeviceName,
            App = appium.App,
        };

        if (!string.IsNullOrWhiteSpace(appium.PlatformVersion))
        {
            options.AddAdditionalAppiumOption("platformVersion", appium.PlatformVersion);
        }

        options.AddAdditionalAppiumOption("noReset", appium.NoReset);
        options.AddAdditionalAppiumOption("newCommandTimeout", appium.NewCommandTimeoutSec);

        var commandTimeout = TimeSpan.FromMilliseconds(defaultTimeoutMs);
        var serverUri = new Uri(appium.ServerUrl);

        return string.Equals(appium.PlatformName, "iOS", StringComparison.OrdinalIgnoreCase)
            ? new IOSDriver(serverUri, options, commandTimeout)
            : new AndroidDriver(serverUri, options, commandTimeout);
    }
}
