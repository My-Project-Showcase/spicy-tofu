using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Application.Automation;
using Application.Automation.Mobile;
using Application.Automation.Web;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Events;
using Domain.Events.EventsRegistry;
using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile;
using Infrastructure.Automation.Web;
using Infrastructure.Events;
using Infrastructure.Logging;
using Infrastructure.Runtime.EventsService;
using Infrastructure.Runtime.JsonService;
using Infrastructure.Runtime.RunServices;
using Infrastructure.Runtime.TestExecution;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServices(configuration);
        return services;
    }

    public static IServiceCollection AddAutomation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var platform = configuration["SpicyTofu:Platform"];

        if (string.Equals(platform, "Web", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<BrowserHost>();
            services.AddSingleton<WebDriver>();
            services.AddSingleton<IWebDriver>(sp => sp.GetRequiredService<WebDriver>());
            services.AddSingleton<IAutomationDriver>(sp => sp.GetRequiredService<WebDriver>());
        }
        else if (string.Equals(platform, "Mobile", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<MobileHost>();
            services.AddSingleton<MobileDriver>();
            services.AddSingleton<IMobileDriver>(sp => sp.GetRequiredService<MobileDriver>());
            services.AddSingleton<IAutomationDriver>(sp => sp.GetRequiredService<MobileDriver>());
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown or missing SpicyTofu:Platform '{platform ?? "<missing>"}'. Set it to 'Web' or 'Mobile'.");
        }

        return services;
    }

    public static IServiceCollection AddServices(
        this IServiceCollection service,
        IConfiguration configuration)
    {
        service.Configure<LoggingConfig>(configuration.GetSection("Logging"));

        service.AddSingleton<ILogger, Logger>();
        service.AddSingleton<IPrintStrategy, ConsolePrintStrategy>();

        service.AddSingleton<IJsonService, JsonService>();
        service.AddSingleton<TestsLoadedHandler>();
        service.AddSingleton<IRunService, RunService>();
        service.AddSingleton<IEventRegistry, EventService>();
        service.AddEvents();
        return service;
    }

    public static IServiceCollection AddEvents(
        this IServiceCollection service
    )
    {
        service.AddSingleton<ITestEvent, ClickEvent>();

        return service;
    }
}
