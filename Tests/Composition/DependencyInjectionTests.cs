using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

using Application.Automation;
using Application.Elements;
using Application.Locators;
using Application.Runtime.RunService;

using Domain.Events.EventsRegistry;
using Domain.Runtime.Environment.Configuration;

using Infrastructure.Extensions;

namespace Tests.Composition;

public sealed class DependencyInjectionTests
{
    [Theory]
    [InlineData("Web")]
    [InlineData("Mobile")]
    public void Host_ResolvesPlatformServicesAndDisposes(string platform)
    {
        using IHost host = Host.CreateDefaultBuilder(Array.Empty<string>())
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(Settings(platform)))
            .ConfigureServices((context, services) =>
            {
                services.Configure<TestExecution>(context.Configuration.GetSection("TestExecution"));
                services.Configure<PlaywrightConfig>(context.Configuration.GetSection("Playwright"));
                services.Configure<AppiumConfig>(context.Configuration.GetSection("Appium"));
                services.AddInfrastructureDependencies(context.Configuration);
                services.AddAutomation(context.Configuration);
            })
            .Build();

        Assert.NotNull(host.Services.GetRequiredService<IRunService>());
        Assert.NotNull(host.Services.GetRequiredService<IEventRegistry>());
        Assert.NotNull(host.Services.GetRequiredService<IElementRepository>());
        Assert.NotNull(host.Services.GetRequiredService<ILocatorResolver>());
        Assert.NotNull(host.Services.GetRequiredService<INavigator>());
        Assert.NotNull(host.Services.GetRequiredService<IAutomationDriver>());
    }

    [Fact]
    public void AddAutomation_UnknownPlatform_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(Settings("Desktop"))
            .Build();

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddAutomation(configuration));
    }

    private static Dictionary<string, string?> Settings(string platform)
        => new()
        {
            ["SpicyTofu:Platform"] = platform,
            ["Playwright:Browser"] = "Chromium",
            ["Appium:ServerUrl"] = "http://127.0.0.1:4723",
            ["Appium:PlatformName"] = "Android",
            ["Appium:AutomationName"] = "UiAutomator2",
            ["Appium:DeviceName"] = "Pixel_API_34",
            ["Appium:App"] = "./app.apk",
        };
}
